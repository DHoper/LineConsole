import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

import { MenuAction } from "../../types";

interface Props {
  value: MenuAction;
  onChange: (val: MenuAction) => void;
}

export function ActionSelector({ value, onChange }: Props) {
    const handleTypeChange = (type: MenuAction["type"]) => {
        switch (type) {
        case "message":
            onChange({ type, text: "" });
            break;
        case "uri":
            onChange({ type, uri: "" });
            break;
        case "richmenuswitch":
            onChange({ type, richMenuAliasId: "" });
            break;
        case "postback":
            onChange({ type, data: "" });
            break;
        default:
            onChange({ type: "none" });
            break;
        }
    };

    return (
        <div className="space-y-2">
            <Select value={value.type} onValueChange={handleTypeChange}>
                <SelectTrigger>
                    <SelectValue placeholder="請選擇動作類型" />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value="none">無動作</SelectItem>
                    <SelectItem value="message">傳送文字</SelectItem>
                    <SelectItem value="uri">開啟網址</SelectItem>
                    <SelectItem value="richmenuswitch">切換選單</SelectItem>
                    <SelectItem value="postback">Postback</SelectItem>
                </SelectContent>
            </Select>

            {value.type === "message" && (
                <Input
                    placeholder="輸入要傳送的訊息"
                    value={value.text ?? ""}
                    onChange={(e) => onChange({ type: "message", text: e.target.value })}
                />
            )}
            {value.type === "uri" && (
                <Input
                    placeholder="輸入網址（https://...）"
                    value={value.uri ?? ""}
                    onChange={(e) => onChange({ type: "uri", uri: e.target.value })}
                />
            )}
            {value.type === "richmenuswitch" && (
                <Input
                    placeholder="輸入 richMenuAliasId"
                    value={value.richMenuAliasId ?? ""}
                    onChange={(e) =>
                        onChange({ type: "richmenuswitch", richMenuAliasId: e.target.value })
                    }
                />
            )}
            {value.type === "postback" && (
                <Input
                    placeholder="輸入 data"
                    value={value.data ?? ""}
                    onChange={(e) => onChange({ type: "postback", data: e.target.value })}
                />
            )}
        </div>
    );
}
