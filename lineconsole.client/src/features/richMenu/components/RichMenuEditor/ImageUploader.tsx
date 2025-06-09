import { Upload, X } from "lucide-react";
import { ChangeEvent, useRef, useState } from "react";

import { Button } from "@/components/ui/button";

interface Props {
    imageUrl: string;
    setImageUrl: (url: string) => void;
    setFile: (file: File | null) => void;
}

export function ImageUploader({ imageUrl, setImageUrl, setFile }: Props) {
    const inputRef = useRef<HTMLInputElement>(null);
    const [compressedNotice, setCompressedNotice] = useState(false);
    const [originalFileSize, setOriginalFileSize] = useState<number | null>(null); // 單位：byte

    const MAX_FILE_SIZE_MB = 1;

    const handleFileChange = async (e: ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setOriginalFileSize(file.size); // 記錄原始大小

        // 建立圖片
        const img = new Image();
        const reader = new FileReader();

        reader.onload = async () => {
            if (typeof reader.result !== "string") return;
            img.src = reader.result;

            img.onload = async () => {
                const canvas = document.createElement("canvas");
                canvas.width = img.width;
                canvas.height = img.height;

                const ctx = canvas.getContext("2d");
                if (!ctx) return;

                ctx.drawImage(img, 0, 0);

                let quality = 0.92;
                let blob: Blob | null = null;

                do {
                    blob = await new Promise<Blob | null>((resolve) =>
                        canvas.toBlob((b) => resolve(b), "image/jpeg", quality)
                    );

                    if (!blob) break;
                    if (blob.size / 1024 / 1024 <= MAX_FILE_SIZE_MB) break;
                    quality -= 0.05;
                } while (quality > 0.3);

                if (!blob) return;

                const compressedFile = new File([blob], file.name, { type: "image/jpeg" });

                const previewUrl = URL.createObjectURL(compressedFile);
                setImageUrl(previewUrl);
                setFile(compressedFile);
                setCompressedNotice(true);
            };
        };

        reader.readAsDataURL(file);
    };

    const handleReset = () => {
        setImageUrl("");
        setFile(null);
        setCompressedNotice(false);
        setOriginalFileSize(null);
        if (inputRef.current) {
            inputRef.current.value = "";
        }
    };

    const formatBytes = (bytes: number) => {
        return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
    };

    return (
        <div className="space-y-2">
            <div className="flex items-center justify-between">
                <label className="text-sm font-medium text-gray-700">
                    圖文選單圖片（建議尺寸：2500x1686 或 2500x843）
                </label>

                {imageUrl && (
                    <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        className="text-red-500"
                        onClick={handleReset}
                    >
                        <X className="w-4 h-4 mr-1" />
                        移除圖片
                    </Button>
                )}
            </div>

            <input
                ref={inputRef}
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="hidden"
                id="richmenu-image-upload"
            />

            <Button
                type="button"
                variant="outline"
                onClick={() => inputRef.current?.click()}
            >
                <Upload className="w-4 h-4 mr-2" />
                {imageUrl ? "重新上傳圖片" : "上傳圖片"}
            </Button>

            {compressedNotice && originalFileSize !== null && (
                <p className="text-xs text-yellow-600">
                    圖片已自動壓縮為低於 1MB（JPG 格式）。原始大小：{formatBytes(originalFileSize)}
                </p>
            )}
        </div>
    );
}
