import { axiosClient } from "@/libs/http/axiosClient";
import { ApiResponse } from "@/types/api";
import {
    RichMenuIdResult,
    RichMenuListResult,
    RichMenuResult,
    RichMenuWithImageResult,
    CreateRichMenuRequest
} from "../types";

const getBaseUrl = (lineOfficialAccountId: string) =>
    `/api/accounts/${lineOfficialAccountId}/richmenu`;

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
        request: CreateRichMenuRequest
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

        if (request.scheduleStart)
            formData.append("ScheduleStart", request.scheduleStart);
        if (request.scheduleEnd)
            formData.append("ScheduleEnd", request.scheduleEnd);

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

    getDefault: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<RichMenuIdResult>> => {
        const res = await axiosClient.get(`${getBaseUrl(lineOfficialAccountId)}/default`);
        return res.data;
    },

    clearDefault: async (
        lineOfficialAccountId: string
    ): Promise<ApiResponse<null>> => {
        const res = await axiosClient.delete(`${getBaseUrl(lineOfficialAccountId)}/default`);
        return res.data;
    },
};
