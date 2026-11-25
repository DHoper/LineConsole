import { ApiResponse } from "@/features/common/types/api";
import { axiosClient } from "@/libs/http/axiosClient";

import {
    CreateRichMenuInput,
    CreateRichMenuScheduleInput,
    RichMenuIdResult,
    RichMenuListResult,
    RichMenuResult,
    RichMenuSchedule,
    RichMenuWithImageResult} from "../types";

const getBaseUrl = (lineOfficialAccountId: string) =>
    `/accounts/${lineOfficialAccountId}/richmenu`;

export const richMenuAPI = {
    list: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<RichMenuListResult>> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/list`);
        return res.data;
    },

    listWithPreview: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<RichMenuWithImageResult[]>> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/list-with-preview`);
        return res.data;
    },

    getById: async (
        lineOfficialAccountId: string,
        richMenuId: string
    ): Promise<ApiResponse<RichMenuResult>> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/${richMenuId}`);
        return res.data;
    },

    getImage: async (
        lineOfficialAccountId: string,
        richMenuId: string
    ): Promise<Blob> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/${richMenuId}/content`, {
            responseType: "blob",
        });
        return res.data;
    },

    createWithImage: async (
        lineOfficialAccountId: string,
        request: CreateRichMenuInput
    ): Promise<ApiResponse<RichMenuIdResult>> => {
        const formData = new FormData();

        formData.append("Selected", request.selected.toString());
        formData.append("Name", request.name);
        formData.append("ChatBarText", request.chatBarText);
        formData.append("Size.Width", request.size.width.toString());
        formData.append("Size.Height", request.size.height.toString());

        request.areas.forEach((area, i) => {
            formData.append(`Areas[${i}].Bounds.X`, area.bounds.x.toString());
            formData.append(`Areas[${i}].Bounds.Y`, area.bounds.y.toString());
            formData.append(`Areas[${i}].Bounds.Width`, area.bounds.width.toString());
            formData.append(`Areas[${i}].Bounds.Height`, area.bounds.height.toString());

            formData.append(`Areas[${i}].Action.Type`, area.action.type);
            if (area.action.text) formData.append(`Areas[${i}].Action.Text`, area.action.text);
            if (area.action.data) formData.append(`Areas[${i}].Action.Data`, area.action.data);
            if (area.action.uri) formData.append(`Areas[${i}].Action.Uri`, area.action.uri);
            if (area.action.richMenuAliasId)
                formData.append(`Areas[${i}].Action.RichMenuAliasId`, area.action.richMenuAliasId);
            if (area.action.dateTime)
                formData.append(`Areas[${i}].Action.Datetime`, area.action.dateTime);
        });

        formData.append("Image", request.image);

        const res = await axiosClient.post(`${getBaseUrl(lineOfficialAccountId)}/with-image`, formData);
        return res.data;
    },

    delete: async (
        lineOfficialAccountId: string,
        richMenuId: string
    ): Promise<ApiResponse<null>> => {
        const res = await axiosClient.delete(`${getBaseUrl(lineOfficialAccountId)}/${richMenuId}`);
        return res.data;
    },

    setDefault: async (
        lineOfficialAccountId: string,
        richMenuId: string
    ): Promise<ApiResponse<null>> => {
        const res = await axiosClient.post(`${getBaseUrl(lineOfficialAccountId)}/default/${richMenuId}`);
        return res.data;
    },

    /** 沒有預設選單時 data 是 null */
    getDefault: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<RichMenuIdResult | null>> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/default`);
        return res.data;
    },

    schedules: async (lineOfficialAccountId: string): Promise<ApiResponse<RichMenuSchedule[]>> =>
        (await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/schedules`)).data,

    createSchedule: async (
        lineOfficialAccountId: string,
        input: CreateRichMenuScheduleInput
    ): Promise<ApiResponse<RichMenuSchedule>> =>
        (await axiosClient.post(`${getBaseUrl(lineOfficialAccountId)}/schedules`, input)).data,

    cancelSchedule: async (lineOfficialAccountId: string, scheduleId: string): Promise<ApiResponse<null>> =>
        (await axiosClient.delete(`${getBaseUrl(lineOfficialAccountId)}/schedules/${scheduleId}`)).data,

    clearDefault: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<null>> => {
        const res = await axiosClient.delete(`${getBaseUrl(lineOfficialAccountId)}/default`);
        return res.data;
    },
};
