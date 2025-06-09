import { Plus } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";

import { richMenuAPI } from "../api/richMenuAPI";
import {
    useClearDefaultRichMenu,
    useDeleteRichMenu,
    useSetDefaultRichMenu,
} from "../api/useRichMenuMutation";
import {
    useDefaultRichMenu,
    useRichMenuById,
    useRichMenuList,
} from "../api/useRichMenuQuery";
import RichMenuCard from "../components/RichMenuCard";
import RichMenuEditor from "../components/RichMenuEditor";
import { RichMenu } from "../types";

export default function RichMenuListPage() {
    const [inputToken, setInputToken] = useState("");
    const [confirmedToken, setConfirmedToken] = useState("");
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [editingImage, setEditingImage] = useState<string>("");
    const [imageErrors, setImageErrors] = useState<Record<string, boolean>>({});

    const { data, isLoading, isError } = useRichMenuList(confirmedToken);
    const { data: defaultMenu } = useDefaultRichMenu(confirmedToken, !!confirmedToken);
    const { data: editingData } = useRichMenuById(confirmedToken, editingId || "", !!editingId);

    const deleteMutation = useDeleteRichMenu(confirmedToken);
    const setDefaultMutation = useSetDefaultRichMenu(confirmedToken);
    const clearDefaultMutation = useClearDefaultRichMenu(confirmedToken);

    const handleConfirmToken = () => {
        if (!inputToken.trim()) {
            toast.error("請輸入有效的 Access Token");
            return;
        }
        setConfirmedToken(inputToken.trim());
    };

    const handleToggleDefault = (id: string) => {
        const currentId = defaultMenu?.data?.richMenuId;
        if (id === currentId) {
            clearDefaultMutation.mutate(undefined, {
                onError: () => toast.error("清除預設選單失敗"),
                onSuccess: () => toast.success("預設選單已清除"),
            });
        } else {
            setDefaultMutation.mutate(id, {
                onError: () => toast.error("設定預設選單失敗"),
                onSuccess: () => toast.success("預設選單已更新"),
            });
        }
    };

    const handleDelete = (id: string) => {
        deleteMutation.mutate(id, {
            onError: () => toast.error("刪除失敗"),
            onSuccess: () => toast.success("已刪除選單"),
        });
    };

    const handleEdit = async (id: string) => {
        try {
            const blob = await richMenuAPI.getImage(confirmedToken, id);
            const reader = new FileReader();

            reader.onloadend = () => {
                if (typeof reader.result === "string") {
                    setEditingImage(reader.result);
                    setEditingId(id);
                    setDialogOpen(true);
                }
            };

            reader.readAsDataURL(blob);
        } catch {
            toast.error("圖片載入失敗");
        }
    };

    const isEditing = !!editingId;

    return (
        <div className="p-6 space-y-6 max-w-6xl mx-auto">
            <div className="space-y-4">
                <h1 className="text-2xl font-bold">圖文選單管理</h1>
                <div className="flex gap-2">
                    <Input
                        placeholder="請輸入 LINE Channel Access Token"
                        value={inputToken}
                        onChange={(e) => setInputToken(e.target.value)}
                        className="w-full md:w-1/2"
                    />
                    <Button onClick={handleConfirmToken}>確認</Button>
                </div>
            </div>

            <div className="pt-6 space-y-4">
                {!confirmedToken ? (
                    <p className="text-sm text-gray-500">請輸入 Access Token 並按下「確認」以查詢圖文選單</p>
                ) : isLoading ? (
                    <p>載入中...</p>
                ) : isError ? (
                    <p className="text-red-500">載入失敗，請確認 Token 是否正確</p>
                ) : (
                    <>
                        <div className="flex justify-end">
                            <Dialog open={dialogOpen} onOpenChange={(open) => {
                                setDialogOpen(open);
                                if (!open) {
                                    setEditingId(null);
                                    setEditingImage("");
                                }
                            }}>
                                <DialogTrigger asChild>
                                    <Button onClick={() => setEditingId(null)}>
                                        <Plus className="w-4 h-4 mr-2" />
                                        新增選單
                                    </Button>
                                </DialogTrigger>
                                <DialogContent className="max-w-5xl">
                                    <DialogHeader>
                                        <DialogTitle>{isEditing ? "編輯選單" : "新增圖文選單"}</DialogTitle>
                                    </DialogHeader>
                                    <RichMenuEditor
                                        accessToken={confirmedToken}
                                        initialData={editingData?.data as RichMenu}
                                        initialImageUrl={editingImage}
                                        onSuccess={() => setDialogOpen(false)}
                                    />
                                </DialogContent>
                            </Dialog>
                        </div>

                        {data?.data?.length ? (
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                                {data.data.map((menu) => {
                                    const isDefault = defaultMenu?.data?.richMenuId === menu.richMenuId;
                                    const hasError = imageErrors[menu.richMenuId] || !menu.imageBase64;

                                    return (
                                        <RichMenuCard
                                            key={menu.richMenuId}
                                            menu={menu}
                                            isDefault={isDefault}
                                            hasImageError={hasError}
                                            onEdit={() => handleEdit(menu.richMenuId)}
                                            onDelete={() => handleDelete(menu.richMenuId)}
                                            onToggleDefault={() => handleToggleDefault(menu.richMenuId)}
                                            onImageError={() => {
                                                setImageErrors((prev) => ({
                                                    ...prev,
                                                    [menu.richMenuId]: true,
                                                }));
                                            }}
                                        />
                                    );
                                })}
                            </div>
                        ) : (
                            <div className="text-center py-20 text-gray-500 text-sm border rounded bg-white shadow">
                                尚未建立任何圖文選單
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}
