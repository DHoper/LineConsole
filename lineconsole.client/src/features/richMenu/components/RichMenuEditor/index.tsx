import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";

import { richMenuAPI } from "../../api/richMenuAPI";
import { useCreateRichMenuWithImage } from "../../api/useRichMenuMutation";
import { useRichMenuById } from "../../api/useRichMenuQuery";
import {
    RichMenu,
    MenuAction,
    MenuArea,
    MenuSize,
} from "../../types";
import { AreaConfigList } from "./AreaConfigList";
import { ImageGridEditor } from "./ImageGridEditor";
import { ImageUploader } from "./ImageUploader";
import { PresetLayoutDialog } from "./PresetLayoutDialog";

interface RichMenuEditorProps {
    accessToken: string;
    onSuccess?: () => void;
    richMenuId?: string;
    initialData?: RichMenu;
    initialImageUrl?: string;
}

export default function RichMenuEditor({
    accessToken,
    onSuccess,
    richMenuId,
}: RichMenuEditorProps) {
    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imageUrl, setImageUrl] = useState<string>("");

    const createMutation = useCreateRichMenuWithImage(accessToken);
    const [menu, setMenu] = useState<RichMenu>({
        name: "",
        chatBarText: "",
        selected: false,
        size: { width: 2500, height: 1686 },
        areas: [],
    });

    const { data: menuData } = useRichMenuById(accessToken, richMenuId || "", !!richMenuId);

    // 編輯模式載入選單資料
    useEffect(() => {
        if (menuData?.data) {
            setMenu(menuData.data);
        }
    }, [menuData]);

    // 編輯模式載入圖片
    useEffect(() => {
        if (!richMenuId) return;
        richMenuAPI
            .getImage(accessToken, richMenuId)
            .then(setImageUrl)
            .catch(() => toast.error("載入選單圖片失敗"));
    }, [accessToken, richMenuId]);

    const handleAddArea = (area: MenuArea) => {
        setMenu((prev) => ({ ...prev, areas: [...prev.areas, area] }));
    };

    const handleRemoveArea = (index: number) => {
        setMenu((prev) => ({ ...prev, areas: prev.areas.filter((_, i) => i !== index) }));
    };

    const handleUpdateArea = (index: number, action: MenuAction) => {
        setMenu((prev) => ({
            ...prev,
            areas: prev.areas.map((a, i) => (i === index ? { ...a, action } : a)),
        }));
    };

    const handleSave = () => {
        if (!imageFile) {
            toast.error("請先上傳圖片");
            return;
        }

        createMutation.mutate(
            { richMenu: menu, imageFile },
            {
                onSuccess: () => {
                    toast.success("選單已成功儲存");
                    setImageFile(null);
                    setImageUrl("");
                    setMenu({
                        name: "",
                        chatBarText: "",
                        selected: false,
                        size: { width: 2500, height: 1686 },
                        areas: [],
                    });
                    onSuccess?.();
                },
                onError: () => {
                    toast.error("儲存失敗，請確認資料與圖片大小限制");
                },
            }
        );
    };

    return (
        <div className="space-y-6 max-h-[90vh] overflow-y-auto p-1">
            {/* 基本設定 */}
            <div className="grid md:grid-cols-2 gap-6 border p-6 bg-white rounded shadow-sm">
                <div className="space-y-3">
                    <Label>圖文選單名稱</Label>
                    <Input
                        value={menu.name}
                        onChange={(e) => setMenu((prev) => ({ ...prev, name: e.target.value }))}
                        placeholder="請輸入圖文選單名稱"
                    />

                    <Label>聊天列文字</Label>
                    <Input
                        value={menu.chatBarText}
                        onChange={(e) =>
                            setMenu((prev) => ({ ...prev, chatBarText: e.target.value }))
                        }
                        placeholder="請輸入聊天列底部文字"
                    />
                </div>

                <div className="space-y-3">
                    <Label>是否為預設選單</Label>
                    <Switch
                        checked={menu.selected}
                        onCheckedChange={(checked) =>
                            setMenu((prev) => ({ ...prev, selected: checked }))
                        }
                    />
                </div>
            </div>

            {/* 圖片與分區劃分 */}
            <div className="space-y-4 border p-6 bg-white rounded shadow-sm">
                <div className="flex justify-between items-center">
                    <h2 className="text-lg font-semibold">圖片與分割設定</h2>
                    <PresetLayoutDialog
                        trigger={<Button variant="outline">選擇預設版型</Button>}
                        onApply={(areas: MenuArea[], size: MenuSize) => {
                            setMenu((prev) => ({ ...prev, areas, size }));
                        }}
                    />
                </div>

                <ImageUploader
                    imageUrl={imageUrl}
                    setImageUrl={setImageUrl}
                    setFile={setImageFile}
                />

                {imageUrl && (
                    <ImageGridEditor
                        imageUrl={imageUrl}
                        size={menu.size}
                        areas={menu.areas}
                        onAreaAdd={handleAddArea}
                        onAreaRemove={(_, index) => handleRemoveArea(index)}
                        onAreaClick={(area) => console.log("點擊區塊", area)}
                        onOverlap={() => toast.error("區塊與其他分區重疊，請重新劃分")}
                    />
                )}
            </div>

            {/* 分區設定 */}
            <div className="border p-6 bg-white rounded shadow-sm">
                <h2 className="text-lg font-semibold mb-4">分割區塊設定</h2>
                <AreaConfigList areas={menu.areas} onUpdate={handleUpdateArea} />
            </div>

            {/* 儲存 */}
            <div className="text-right">
                <Button onClick={handleSave} disabled={createMutation.isPending}>
                    {createMutation.isPending ? "儲存中..." : "儲存選單"}
                </Button>
            </div>
        </div>
    );
}
