import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

import type { MenuAction } from "../../types";

interface Props {
  open: boolean;
  onClose: () => void;
  label: string;
  action: MenuAction;
  onChange: (data: { label: string; action: MenuAction }) => void;
}

export function AreaSettingsDialog({ open, onClose, label, action, onChange }: Props) {
    const handleTypeChange = (type: MenuAction["type"]) => {
        let nextAction: MenuAction;
        switch (type) {
        case "message":
            nextAction = { type: "message", text: "" };
            break;
        case "uri":
            nextAction = { type: "uri", uri: "" };
            break;
        case "richmenuswitch":
            nextAction = { type: "richmenuswitch", richMenuAliasId: "" };
            break;
        case "postback":
            nextAction = { type: "postback", data: "" };
            break;
        default:
            nextAction = { type: "none" };
            break;
        }
        onChange({ label, action: nextAction });
    };

    const handleActionChange = <K extends keyof MenuAction>(
        key: K,
        value: MenuAction[K]
    ) => {
        onChange({ label, action: { ...action, [key]: value } });
    };

    return (
        <Dialog open={open} onOpenChange={(v) => !v && onClose()}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle>區塊設定</DialogTitle>
                </DialogHeader>

                <div className="space-y-4">
                    {/* 顯示文字 */}
                    <div>
                        <Label>顯示文字</Label>
                        <Input
                            value={label}
                            onChange={(e) => onChange({ label: e.target.value, action })}
                            placeholder="請輸入顯示文字"
                        />
                    </div>

                    {/* 動作類型 */}
                    <div>
                        <Label>觸發動作</Label>
                        <Select value={action.type} onValueChange={handleTypeChange}>
                            <SelectTrigger>
                                <SelectValue placeholder="選擇一個動作類型" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="none">無動作</SelectItem>
                                <SelectItem value="message">傳送文字訊息</SelectItem>
                                <SelectItem value="uri">開啟網址</SelectItem>
                                <SelectItem value="richmenuswitch">切換選單</SelectItem>
                                <SelectItem value="postback">Postback</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    {/* 動作內容 */}
                    {action.type === "message" && (
                        <div>
                            <Label>文字內容</Label>
                            <Input
                                value={action.text ?? ""}
                                onChange={(e) => handleActionChange("text", e.target.value)}
                                placeholder="請輸入文字"
                            />
                        </div>
                    )}

                    {action.type === "uri" && (
                        <div>
                            <Label>網址</Label>
                            <Input
                                value={action.uri ?? ""}
                                onChange={(e) => handleActionChange("uri", e.target.value)}
                                placeholder="https://"
                            />
                        </div>
                    )}

                    {action.type === "richmenuswitch" && (
                        <>
                            <div>
                                <Label>Rich Menu Alias ID</Label>
                                <Input
                                    value={action.richMenuAliasId ?? ""}
                                    onChange={(e) =>
                                        handleActionChange("richMenuAliasId", e.target.value)
                                    }
                                    placeholder="別名 ID"
                                />
                            </div>
                        </>
                    )}

                    {action.type === "postback" && (
                        <div>
                            <Label>資料參數 (data)</Label>
                            <Input
                                value={action.data ?? ""}
                                onChange={(e) => handleActionChange("data", e.target.value)}
                                placeholder="postback data"
                            />
                        </div>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    );
}
