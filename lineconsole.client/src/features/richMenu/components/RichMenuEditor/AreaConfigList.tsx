import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";

import type { MenuAction, MenuArea } from "../../types";

interface Props {
  areas: MenuArea[];
  onUpdate: (index: number, newAction: MenuAction) => void;
}

export function AreaConfigList({ areas, onUpdate }: Props) {
    const handleTypeChange = (
        index: number,
        type: MenuAction["type"]
    ) => {
        let newAction: MenuAction;

        switch (type) {
        case "message":
            newAction = { type: "message", text: "" };
            break;
        case "uri":
            newAction = { type: "uri", uri: "" };
            break;
        case "richmenuswitch":
            newAction = { type: "richmenuswitch", richMenuAliasId: "" };
            break;
        case "postback":
            newAction = { type: "postback", data: "" };
            break;
        default:
            newAction = { type: "none" };
            break;
        }

        onUpdate(index, newAction);
    };

    return (
        <div className="space-y-6">
            {areas.map((area, idx) => (
                <div
                    key={`${area.bounds.x}-${area.bounds.y}-${idx}`}
                    className="border bg-white p-6 rounded-md shadow-sm space-y-4"
                >
                    <div className="flex items-center justify-between">
                        <h3 className="text-base font-semibold text-primary">
              分割區塊 {idx + 1}
                        </h3>
                        <span className="text-sm text-muted-foreground">
              座標：({area.bounds.x}, {area.bounds.y}) - {area.bounds.width} ×{" "}
                            {area.bounds.height}
                        </span>
                    </div>

                    {/* 動作選擇 */}
                    <div>
                        <Label className="mb-1 block">動作類型</Label>
                        <Select
                            value={area.action.type}
                            onValueChange={(value: MenuAction["type"]) =>
                                handleTypeChange(idx, value)
                            }
                        >
                            <SelectTrigger>
                                <SelectValue placeholder="請選擇動作" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="none">無動作</SelectItem>
                                <SelectItem value="message">傳送訊息</SelectItem>
                                <SelectItem value="uri">開啟網址</SelectItem>
                                <SelectItem value="richmenuswitch">切換選單</SelectItem>
                                <SelectItem value="postback">Postback</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    {/* 動作參數欄位 */}
                    {area.action.type === "message" && (
                        <div>
                            <Label className="mb-1 block">訊息內容</Label>
                            <Textarea
                                placeholder="請輸入要傳送的訊息"
                                value={area.action.text ?? ""}
                                onChange={(e) =>
                                    onUpdate(idx, { ...area.action, text: e.target.value })
                                }
                            />
                        </div>
                    )}

                    {area.action.type === "uri" && (
                        <div>
                            <Label className="mb-1 block">網址</Label>
                            <Input
                                placeholder="https://example.com"
                                value={area.action.uri ?? ""}
                                onChange={(e) =>
                                    onUpdate(idx, { ...area.action, uri: e.target.value })
                                }
                            />
                        </div>
                    )}

                    {area.action.type === "richmenuswitch" && (
                        <div>
                            <Label className="mb-1 block">Rich Menu Alias ID</Label>
                            <Input
                                placeholder="請輸入 richMenuAliasId"
                                value={area.action.richMenuAliasId ?? ""}
                                onChange={(e) =>
                                    onUpdate(idx, {
                                        ...area.action,
                                        richMenuAliasId: e.target.value,
                                    })
                                }
                            />
                        </div>
                    )}

                    {area.action.type === "postback" && (
                        <div>
                            <Label className="mb-1 block">Postback Data</Label>
                            <Input
                                placeholder="請輸入 postback data"
                                value={area.action.data ?? ""}
                                onChange={(e) =>
                                    onUpdate(idx, {
                                        ...area.action,
                                        data: e.target.value,
                                    })
                                }
                            />
                        </div>
                    )}
                </div>
            ))}
        </div>
    );
}
