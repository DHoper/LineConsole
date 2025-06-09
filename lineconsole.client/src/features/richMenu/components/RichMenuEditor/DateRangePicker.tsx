import { format, isValid, parseISO } from "date-fns";
import { Calendar as CalendarIcon } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Label } from "@/components/ui/label";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/libs/ui/utils";

interface Props {
    startAt: string;
    endAt: string;
    onChange: (val: { startAt: string; endAt: string }) => void;
}

export function DateRangePicker({ startAt, endAt, onChange }: Props) {
    const [open, setOpen] = useState(false);

    const parsedStart = isValid(parseISO(startAt)) ? parseISO(startAt) : undefined;
    const parsedEnd = isValid(parseISO(endAt)) ? parseISO(endAt) : undefined;

    return (
        <div className="space-y-2">
            <Label className="text-sm">排程時間</Label>
            <Popover open={open} onOpenChange={setOpen}>
                <PopoverTrigger asChild>
                    <Button
                        variant="outline"
                        className={cn("w-full justify-start text-left font-normal")}
                    >
                        <CalendarIcon className="mr-2 h-4 w-4" />
                        {parsedStart && parsedEnd ? (
                            `${format(parsedStart, "yyyy/MM/dd")} ~ ${format(parsedEnd, "yyyy/MM/dd")}`
                        ) : (
                            <span className="text-muted-foreground">請選擇日期區間</span>
                        )}
                    </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                    <Calendar
                        mode="range"
                        selected={{ from: parsedStart, to: parsedEnd }}
                        onSelect={(range: { from?: Date; to?: Date } | undefined) => {
                            if (range?.from && range?.to) {
                                onChange({
                                    startAt: range.from.toISOString(),
                                    endAt: range.to.toISOString(),
                                });
                                setOpen(false);
                            }
                        }}
                        numberOfMonths={2}
                        initialFocus
                    />
                </PopoverContent>
            </Popover>
        </div>
    );
}
