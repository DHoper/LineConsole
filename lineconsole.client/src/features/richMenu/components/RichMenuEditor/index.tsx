import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import { richMenuAPI } from "../../api/richMenuAPI";
import { useCreateRichMenuWithImage } from "../../api/useRichMenuMutation";
import { useRichMenuById } from "../../api/useRichMenuQuery";
import { MenuAction, MenuArea, MenuSize, RichMenu } from "../../types";
import { AreaConfigList } from "./AreaConfigList";
import { ImageGridEditor } from "./ImageGridEditor";
import { ImageUploader } from "./ImageUploader";
import { PresetLayoutDialog } from "./PresetLayoutDialog";

interface RichMenuEditorProps {
    accountId: string;
    /** 以這個選單為範本建立新的（LINE 的圖文選單建立後不能修改，所以「編輯」就是複製成新的） */
    templateId?: string;
    onSuccess?: () => void;
}

const EMPTY: RichMenu = {
    name: "",
    chatBarText: "",
    selected: false,
    size: { width: 2500, height: 1686 },
    areas: [],
};

/** LINE 的尺寸規則：寬 800～2500、高至少 250、寬高比至少 1.45；圖片尺寸要跟選單尺寸完全一樣 */
const isValidLineSize = ({ width, height }: MenuSize) =>
    width >= 800 && width <= 2500 && height >= 250 && width / height >= 1.45;

const readImageSize = (url: string) =>
    new Promise<MenuSize>((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve({ width: img.naturalWidth, height: img.naturalHeight });
        img.onerror = reject;
        img.src = url;
    });

const missingAction = (a: MenuArea) =>
    a.action.type === "none" ||
    (a.action.type === "message" && !a.action.text?.trim()) ||
    (a.action.type === "uri" && !a.action.uri?.trim()) ||
    (a.action.type === "postback" && !a.action.data?.trim()) ||
    (a.action.type === "richmenuswitch" && !a.action.richMenuAliasId?.trim());

export default function RichMenuEditor({ accountId, templateId, onSuccess }: RichMenuEditorProps) {
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imageUrl, setImageUrl] = useState<string>("");
    const [menu, setMenu] = useState<RichMenu>(EMPTY);

    const createMutation = useCreateRichMenuWithImage(accountId);
    const { data: template } = useRichMenuById(accountId, templateId ?? "", !!templateId);

    // 範本：帶入名稱、尺寸、區塊和圖片
    useEffect(() => {
        if (!templateId || !template?.data) return;
        const t = template.data;
        setMenu({ ...EMPTY, name: `${t.name}（複製）`, chatBarText: t.chatBarText, size: t.size, areas: t.areas });
        richMenuAPI
            .getImage(accountId, templateId)
            .then((blob) => {
                setImageFile(new File([blob], "template.jpg", { type: blob.type || "image/jpeg" }));
                setImageUrl(URL.createObjectURL(blob));
            })
            .catch(() => toast.error("載入範本圖片失敗"));
    }, [accountId, templateId, template]);

    // 換圖片：圖片尺寸就是選單尺寸；不合 LINE 的規則就不收
    const handleImage = async (url: string) => {
        setImageUrl(url);
        if (!url) return;
        const size = await readImageSize(url);
        if (!isValidLineSize(size)) {
            toast.error(`圖片尺寸 ${size.width}×${size.height} 不符合 LINE 的規定（建議 2500×1686 或 2500×843）`);
            setImageUrl("");
            setImageFile(null);
            return;
        }
        setMenu((prev) => {
            const fits = prev.areas.every((a) => a.bounds.x + a.bounds.width <= size.width && a.bounds.y + a.bounds.height <= size.height);
            if (!fits) toast.info("圖片尺寸變了，原本的分區超出範圍，已清除");
            return { ...prev, size, areas: fits ? prev.areas : [] };
        });
    };

    const handleAddArea = (area: MenuArea) => setMenu((prev) => ({ ...prev, areas: [...prev.areas, area] }));
    const handleRemoveArea = (index: number) =>
        setMenu((prev) => ({ ...prev, areas: prev.areas.filter((_, i) => i !== index) }));
    const handleUpdateArea = (index: number, action: MenuAction) =>
        setMenu((prev) => ({ ...prev, areas: prev.areas.map((a, i) => (i === index ? { ...a, action } : a)) }));

    const handleSave = () => {
        if (!imageFile) return toast.error("請先上傳圖片");
        if (!menu.name.trim()) return toast.error("請輸入選單名稱");
        if (!menu.chatBarText.trim() || menu.chatBarText.length > 14) return toast.error("聊天列文字要 1～14 個字");
        if (menu.areas.length === 0) return toast.error("至少要劃分一個點擊區塊");
        const missing = menu.areas.filter(missingAction).length;
        if (missing > 0) return toast.error(`還有 ${missing} 個區塊沒有設定點擊後的動作`);

        createMutation.mutate(
            { ...menu, image: imageFile },
            {
                onSuccess: () => {
                    toast.success("選單已建立");
                    setImageFile(null);
                    setImageUrl("");
                    setMenu(EMPTY);
                    onSuccess?.();
                },
            },
        );
    };

    return (
        <div className="space-y-6 max-h-[80vh] overflow-y-auto p-1">
            {templateId && (
                <p className="text-sm text-muted-foreground">
                    LINE 的圖文選單建立後不能修改：這裡會以原本的選單為範本，建立一個新的選單。
                </p>
            )}

            {/* 基本設定 */}
            <div className="grid md:grid-cols-2 gap-6 border p-6 bg-white rounded shadow-sm">
                <div className="space-y-2">
                    <Label htmlFor="rm-name">圖文選單名稱（只有後台看得到）</Label>
                    <Input
                        id="rm-name"
                        value={menu.name}
                        maxLength={300}
                        onChange={(e) => setMenu((prev) => ({ ...prev, name: e.target.value }))}
                        placeholder="例如：週年慶選單"
                    />
                </div>
                <div className="space-y-2">
                    <Label htmlFor="rm-chatbar">聊天列文字（最多 14 字，顯示在聊天室下方）</Label>
                    <Input
                        id="rm-chatbar"
                        value={menu.chatBarText}
                        maxLength={14}
                        onChange={(e) => setMenu((prev) => ({ ...prev, chatBarText: e.target.value }))}
                        placeholder="例如：點我看優惠"
                    />
                </div>
            </div>

            {/* 圖片與分區 */}
            <div className="space-y-4 border p-6 bg-white rounded shadow-sm">
                <div className="flex justify-between items-center">
                    <h2 className="text-lg font-semibold">圖片與分區（{menu.size.width}×{menu.size.height}）</h2>
                    <PresetLayoutDialog
                        trigger={<Button variant="outline">套用版型</Button>}
                        onApply={(areas: MenuArea[], size: MenuSize) => setMenu((prev) => ({ ...prev, areas, size }))}
                    />
                </div>

                <ImageUploader imageUrl={imageUrl} setImageUrl={handleImage} setFile={setImageFile} />

                {imageUrl && (
                    <ImageGridEditor
                        imageUrl={imageUrl}
                        size={menu.size}
                        areas={menu.areas}
                        onAreaAdd={handleAddArea}
                        onAreaRemove={handleRemoveArea}
                        onOverlap={() => toast.error("區塊跟其他分區重疊，請重新劃分")}
                    />
                )}
            </div>

            {/* 每個區塊點下去做什麼 */}
            <div className="border p-6 bg-white rounded shadow-sm">
                <h2 className="text-lg font-semibold mb-4">區塊動作（{menu.areas.length} 個）</h2>
                <AreaConfigList areas={menu.areas} onUpdate={handleUpdateArea} />
            </div>

            <div className="text-right">
                <Button onClick={handleSave} disabled={createMutation.isPending}>
                    {createMutation.isPending ? "建立中..." : "建立選單"}
                </Button>
            </div>
        </div>
    );
}
