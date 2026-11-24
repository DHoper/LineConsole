import { ChevronLeft, ChevronRight } from "lucide-react";
import * as React from "react";
import { DayPicker } from "react-day-picker";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/libs/ui/utils";

/**
 * shadcn/ui 的日曆，對應 react-day-picker 9。
 * v9 的 classNames 名稱（month_caption、day_button、range_start…）和箭頭元件（Chevron）都跟 v8 不同。
 */
function Calendar({
    className,
    classNames,
    showOutsideDays = true,
    ...props
}: React.ComponentProps<typeof DayPicker>) {
    const navButton = cn(
        buttonVariants({ variant: "outline" }),
        "size-7 bg-transparent p-0 opacity-50 hover:opacity-100"
    );

    return (
        <DayPicker
            showOutsideDays={showOutsideDays}
            className={cn("p-3", className)}
            classNames={{
                months: "relative flex flex-col sm:flex-row gap-4",
                month: "flex flex-col gap-4",
                month_caption: "flex h-7 items-center justify-center",
                caption_label: "text-sm font-medium",
                nav: "absolute inset-x-0 top-0 z-10 flex items-center justify-between",
                button_previous: navButton,
                button_next: navButton,
                month_grid: "w-full border-collapse",
                weekdays: "flex",
                weekday: "text-muted-foreground w-8 rounded-md text-[0.8rem] font-normal",
                week: "mt-2 flex w-full",
                day: "relative p-0 text-center text-sm focus-within:relative focus-within:z-20",
                day_button: cn(
                    buttonVariants({ variant: "ghost" }),
                    "size-8 p-0 font-normal aria-selected:opacity-100"
                ),
                selected: "[&>button]:bg-primary [&>button]:text-primary-foreground",
                range_start: "rounded-l-md bg-accent",
                range_end: "rounded-r-md bg-accent",
                range_middle: "bg-accent [&>button]:bg-transparent [&>button]:text-accent-foreground",
                today: "[&>button]:bg-accent [&>button]:text-accent-foreground",
                outside: "text-muted-foreground opacity-50",
                disabled: "text-muted-foreground opacity-50",
                hidden: "invisible",
                ...classNames,
            }}
            components={{
                Chevron: ({ orientation, className: iconClass }) =>
                    orientation === "left" ? (
                        <ChevronLeft className={cn("size-4", iconClass)} />
                    ) : (
                        <ChevronRight className={cn("size-4", iconClass)} />
                    ),
            }}
            {...props}
        />
    );
}

export { Calendar };
