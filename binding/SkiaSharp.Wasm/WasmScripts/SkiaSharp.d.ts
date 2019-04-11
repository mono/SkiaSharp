declare namespace SkiaSharp {
    class SurfaceManager {
        static _readInternal: any;
        static _peekInternal: any;
        static _isAtEndInternal: any;
        static _hasPositionInternal: any;
        static _hasLengthInternal: any;
        static _rewindInternal: any;
        static _getPositionInternal: any;
        static _seekInternal: any;
        static _moveInternal: any;
        static _getLengthInternal: any;
        static _createNewInternal: any;
        static _destroyInternal: any;
        static registerManagedStream(): boolean;
        static readInternal(managedStreamPtr: number, buffer: number, size: number): number;
        static peekInternal(managedStreamPtr: number, buffer: number, size: number): number;
        static invalidateCanvas(pData: number, canvasId: string, width: number, height: number): boolean;
    }
    class ApiOverride {
        static sk2MonoMap: {
            [id: number]: any;
        };
        static mono2SkMap: {
            [id: number]: any;
        };
        static bitmapPixels: {
            [id: number]: any;
        };
        static memoryStreamMap: {
            [id: number]: any;
        };
        static memcpy(sourceArray: any, pSource: any, destArray: any, pDest: any, length: number): void;
        static memcpy_Mono2Sk(pMono: any, pSk: any, length: number): void;
        static memcpy_Sk2Mono(pSk: any, pMono: any, length: number): void;
        static sk_bitmap_get_pixels_0_Post(skPixels: any, parms: any, retStruct: any): any;
        static sk_bitmap_destructor_0_Post(ret: any, parms: any): any;
        static sk_codec_get_pixels_0_Pre(parms: any): void;
        static sk_memorystream_set_memory_Pre(parms: any): void;
        static sk_memorystream_destroy_Post(ret: any, parms: any): any;
    }
}
declare const CanvasKit: any;
declare const MonoRuntime: Uno.UI.Interop.IMonoRuntime;
declare const MonoSupport: any;
declare module Uno.UI.Interop {
    interface IMonoRuntime {
        mono_string(str: string): Interop.IMonoStringHandle;
        conv_string(strHandle: Interop.IMonoStringHandle): string;
    }
    interface IMonoStringHandle {
    }
}
declare namespace SkiaSharp {
    class GRGlFramebufferInfo {
        fboId: number;
        format: number;
        static unmarshal(pData: number, memoryContext?: any): GRGlFramebufferInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class GRGlTextureInfo {
        fTarget: number;
        fID: number;
        fFormat: number;
        static unmarshal(pData: number, memoryContext?: any): GRGlTextureInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKCodecFrameInfo {
        requiredFrame: number;
        duration: number;
        fullyRecieved: number;
        alphaType: number;
        disposalMethod: number;
        static unmarshal(pData: number, memoryContext?: any): SKCodecFrameInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKCodecOptionsInternal {
        fZeroInitialized: number;
        fSubset: number;
        fFrameIndex: number;
        fPriorFrame: number;
        fPremulBehavior: number;
        static unmarshal(pData: number, memoryContext?: any): SKCodecOptionsInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKColor {
        color: number;
        static unmarshal(pData: number, memoryContext?: any): SKColor;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKColorSpacePrimaries {
        fRX: number;
        fRY: number;
        fGX: number;
        fGY: number;
        fBX: number;
        fBY: number;
        fWX: number;
        fWY: number;
        static unmarshal(pData: number, memoryContext?: any): SKColorSpacePrimaries;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKColorSpaceTransferFn {
        fG: number;
        fA: number;
        fB: number;
        fC: number;
        fD: number;
        fE: number;
        fF: number;
        static unmarshal(pData: number, memoryContext?: any): SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKDocumentPdfMetadataInternal {
        Title: number;
        Author: number;
        Subject: number;
        Keywords: number;
        Creator: number;
        Producer: number;
        Creation: number;
        Modified: number;
        RasterDPI: number;
        PDFA: number;
        EncodingQuality: number;
        static unmarshal(pData: number, memoryContext?: any): SKDocumentPdfMetadataInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKFontMetrics {
        flags: number;
        top: number;
        ascent: number;
        descent: number;
        bottom: number;
        leading: number;
        avgCharWidth: number;
        maxCharWidth: number;
        xMin: number;
        xMax: number;
        xHeight: number;
        capHeight: number;
        underlineThickness: number;
        underlinePosition: number;
        strikeoutThickness: number;
        strikeoutPosition: number;
        static unmarshal(pData: number, memoryContext?: any): SKFontMetrics;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKHighContrastConfig {
        fGrayscale: number;
        fInvertStyle: number;
        fContrast: number;
        static unmarshal(pData: number, memoryContext?: any): SKHighContrastConfig;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKImageInfoNative {
        fColorSpace: number;
        fWidth: number;
        fHeight: number;
        fColorType: number;
        fAlphaType: number;
        static unmarshal(pData: number, memoryContext?: any): SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKJpegEncoderOptions {
        fQuality: number;
        fDownsample: number;
        fAlphaOption: number;
        fBlendBehavior: number;
        static unmarshal(pData: number, memoryContext?: any): SKJpegEncoderOptions;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKLatticeInternal {
        fXDivs: number;
        fYDivs: number;
        fRectTypes: number;
        fXCount: number;
        fYCount: number;
        fBounds: number;
        fColors: number;
        static unmarshal(pData: number, memoryContext?: any): SKLatticeInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKMask {
        image: number;
        bounds: SkiaSharp.SKRectI;
        rowBytes: number;
        format: number;
        static unmarshal(pData: number, memoryContext?: any): SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKMatrix {
        scaleX: number;
        skewX: number;
        transX: number;
        skewY: number;
        scaleY: number;
        transY: number;
        persp0: number;
        persp1: number;
        persp2: number;
        static unmarshal(pData: number, memoryContext?: any): SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKPMColor {
        color: number;
        static unmarshal(pData: number, memoryContext?: any): SKPMColor;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKPngEncoderOptions {
        fFilterFlags: number;
        fZLibLevel: number;
        fUnpremulBehavior: number;
        fComments: number;
        static unmarshal(pData: number, memoryContext?: any): SKPngEncoderOptions;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKPoint {
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKPoint3 {
        x: number;
        y: number;
        z: number;
        static unmarshal(pData: number, memoryContext?: any): SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKPointI {
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): SKPointI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKRect {
        left: number;
        top: number;
        right: number;
        bottom: number;
        static unmarshal(pData: number, memoryContext?: any): SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKRectI {
        left: number;
        top: number;
        right: number;
        bottom: number;
        static unmarshal(pData: number, memoryContext?: any): SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKSizeI {
        width: number;
        height: number;
        static unmarshal(pData: number, memoryContext?: any): SKSizeI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKTextBlobBuilderRunBuffer {
        glyphs: number;
        pos: number;
        utf8text: number;
        clusters: number;
        static unmarshal(pData: number, memoryContext?: any): SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SKWebpEncoderOptions {
        fCompression: number;
        fQuality: number;
        fUnpremulBehavior: number;
        static unmarshal(pData: number, memoryContext?: any): SKWebpEncoderOptions;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class SkiaApi {
        static sk_colorspace_unref_0(pParams: number, pReturn: number): number;
        static sk_colorspace_gamma_close_to_srgb_0(pParams: number, pReturn: number): number;
        static sk_colorspace_gamma_is_linear_0(pParams: number, pReturn: number): number;
        static sk_colorspace_is_srgb_0(pParams: number, pReturn: number): number;
        static sk_colorspace_gamma_get_type_0(pParams: number, pReturn: number): number;
        static sk_colorspace_gamma_get_gamma_named_0(pParams: number, pReturn: number): number;
        static sk_colorspace_equals_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_srgb_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_srgb_linear_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_icc_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_icc_1(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_gamma_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_gamma_and_gamut_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_coeffs_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_coeffs_and_gamut_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_gamma_named_0(pParams: number, pReturn: number): number;
        static sk_colorspace_new_rgb_with_gamma_named_and_gamut_0(pParams: number, pReturn: number): number;
        static sk_colorspace_to_xyzd50_0(pParams: number, pReturn: number): number;
        static sk_colorspace_as_to_xyzd50_0(pParams: number, pReturn: number): number;
        static sk_colorspace_as_from_xyzd50_0(pParams: number, pReturn: number): number;
        static sk_colorspace_is_numerical_transfer_fn_0(pParams: number, pReturn: number): number;
        static sk_colorspaceprimaries_to_xyzd50_0(pParams: number, pReturn: number): number;
        static sk_colorspace_transfer_fn_invert_0(pParams: number, pReturn: number): number;
        static sk_colorspace_transfer_fn_transform_0(pParams: number, pReturn: number): number;
        static sk_colortype_get_default_8888_0(pParams: number, pReturn: number): number;
        static sk_surface_new_null_0(pParams: number, pReturn: number): number;
        static sk_surface_unref_0(pParams: number, pReturn: number): number;
        static sk_surface_new_raster_0(pParams: number, pReturn: number): number;
        static sk_surface_new_raster_direct_0(pParams: number, pReturn: number): number;
        static sk_surface_get_canvas_0(pParams: number, pReturn: number): number;
        static sk_surface_new_image_snapshot_0(pParams: number, pReturn: number): number;
        static sk_surface_new_backend_render_target_0(pParams: number, pReturn: number): number;
        static sk_surface_new_backend_texture_0(pParams: number, pReturn: number): number;
        static sk_surface_new_backend_texture_as_render_target_0(pParams: number, pReturn: number): number;
        static sk_surface_new_render_target_0(pParams: number, pReturn: number): number;
        static sk_surface_draw_0(pParams: number, pReturn: number): number;
        static sk_surface_peek_pixels_0(pParams: number, pReturn: number): number;
        static sk_surface_read_pixels_0(pParams: number, pReturn: number): number;
        static sk_surface_get_props_0(pParams: number, pReturn: number): number;
        static sk_surfaceprops_new_0(pParams: number, pReturn: number): number;
        static sk_surfaceprops_delete_0(pParams: number, pReturn: number): number;
        static sk_surfaceprops_get_flags_0(pParams: number, pReturn: number): number;
        static sk_surfaceprops_get_pixel_geometry_0(pParams: number, pReturn: number): number;
        static sk_canvas_save_0(pParams: number, pReturn: number): number;
        static sk_canvas_save_layer_0(pParams: number, pReturn: number): number;
        static sk_canvas_save_layer_1(pParams: number, pReturn: number): number;
        static sk_canvas_restore_0(pParams: number, pReturn: number): number;
        static sk_canvas_get_save_count_0(pParams: number, pReturn: number): number;
        static sk_canvas_restore_to_count_0(pParams: number, pReturn: number): number;
        static sk_canvas_translate_0(pParams: number, pReturn: number): number;
        static sk_canvas_scale_0(pParams: number, pReturn: number): number;
        static sk_canvas_rotate_degrees_0(pParams: number, pReturn: number): number;
        static sk_canvas_rotate_radians_0(pParams: number, pReturn: number): number;
        static sk_canvas_skew_0(pParams: number, pReturn: number): number;
        static sk_canvas_concat_0(pParams: number, pReturn: number): number;
        static sk_canvas_quick_reject_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_paint_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_region_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_rect_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_rrect_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_round_rect_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_oval_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_circle_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_path_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_image_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_image_rect_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_image_rect_1(pParams: number, pReturn: number): number;
        static sk_canvas_draw_picture_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_picture_1(pParams: number, pReturn: number): number;
        static sk_canvas_draw_drawable_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_color_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_points_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_point_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_line_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_text_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_text_1(pParams: number, pReturn: number): number;
        static sk_canvas_draw_pos_text_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_pos_text_1(pParams: number, pReturn: number): number;
        static sk_canvas_draw_text_on_path_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_text_on_path_1(pParams: number, pReturn: number): number;
        static sk_canvas_draw_text_blob_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_bitmap_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_bitmap_rect_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_bitmap_rect_1(pParams: number, pReturn: number): number;
        static sk_canvas_reset_matrix_0(pParams: number, pReturn: number): number;
        static sk_canvas_set_matrix_0(pParams: number, pReturn: number): number;
        static sk_canvas_get_total_matrix_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_annotation_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_url_annotation_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_named_destination_annotation_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_link_destination_annotation_0(pParams: number, pReturn: number): number;
        static sk_canvas_clip_rrect_with_operation_0(pParams: number, pReturn: number): number;
        static sk_canvas_clip_rect_with_operation_0(pParams: number, pReturn: number): number;
        static sk_canvas_clip_path_with_operation_0(pParams: number, pReturn: number): number;
        static sk_canvas_clip_region_0(pParams: number, pReturn: number): number;
        static sk_canvas_get_device_clip_bounds_0(pParams: number, pReturn: number): number;
        static sk_canvas_get_local_clip_bounds_0(pParams: number, pReturn: number): number;
        static sk_canvas_new_from_bitmap_0(pParams: number, pReturn: number): number;
        static sk_canvas_flush_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_bitmap_lattice_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_image_lattice_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_bitmap_nine_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_image_nine_0(pParams: number, pReturn: number): number;
        static sk_canvas_destroy_0(pParams: number, pReturn: number): number;
        static sk_canvas_draw_vertices_0(pParams: number, pReturn: number): number;
        static sk_nodraw_canvas_new_0(pParams: number, pReturn: number): number;
        static sk_nodraw_canvas_destroy_0(pParams: number, pReturn: number): number;
        static sk_nway_canvas_new_0(pParams: number, pReturn: number): number;
        static sk_nway_canvas_destroy_0(pParams: number, pReturn: number): number;
        static sk_nway_canvas_add_canvas_0(pParams: number, pReturn: number): number;
        static sk_nway_canvas_remove_canvas_0(pParams: number, pReturn: number): number;
        static sk_nway_canvas_remove_all_0(pParams: number, pReturn: number): number;
        static sk_overdraw_canvas_new_0(pParams: number, pReturn: number): number;
        static sk_overdraw_canvas_destroy_0(pParams: number, pReturn: number): number;
        static sk_paint_new_0(pParams: number, pReturn: number): number;
        static sk_paint_delete_0(pParams: number, pReturn: number): number;
        static sk_paint_reset_0(pParams: number, pReturn: number): number;
        static sk_paint_is_antialias_0(pParams: number, pReturn: number): number;
        static sk_paint_set_antialias_0(pParams: number, pReturn: number): number;
        static sk_paint_is_dither_0(pParams: number, pReturn: number): number;
        static sk_paint_set_dither_0(pParams: number, pReturn: number): number;
        static sk_paint_is_verticaltext_0(pParams: number, pReturn: number): number;
        static sk_paint_set_verticaltext_0(pParams: number, pReturn: number): number;
        static sk_paint_get_color_0(pParams: number, pReturn: number): number;
        static sk_paint_set_color_0(pParams: number, pReturn: number): number;
        static sk_paint_get_style_0(pParams: number, pReturn: number): number;
        static sk_paint_set_style_0(pParams: number, pReturn: number): number;
        static sk_paint_get_stroke_width_0(pParams: number, pReturn: number): number;
        static sk_paint_set_stroke_width_0(pParams: number, pReturn: number): number;
        static sk_paint_get_stroke_miter_0(pParams: number, pReturn: number): number;
        static sk_paint_set_stroke_miter_0(pParams: number, pReturn: number): number;
        static sk_paint_get_stroke_cap_0(pParams: number, pReturn: number): number;
        static sk_paint_set_stroke_cap_0(pParams: number, pReturn: number): number;
        static sk_paint_get_stroke_join_0(pParams: number, pReturn: number): number;
        static sk_paint_set_stroke_join_0(pParams: number, pReturn: number): number;
        static sk_paint_set_shader_0(pParams: number, pReturn: number): number;
        static sk_paint_get_shader_0(pParams: number, pReturn: number): number;
        static sk_paint_set_maskfilter_0(pParams: number, pReturn: number): number;
        static sk_paint_get_maskfilter_0(pParams: number, pReturn: number): number;
        static sk_paint_set_colorfilter_0(pParams: number, pReturn: number): number;
        static sk_paint_get_colorfilter_0(pParams: number, pReturn: number): number;
        static sk_paint_set_imagefilter_0(pParams: number, pReturn: number): number;
        static sk_paint_get_imagefilter_0(pParams: number, pReturn: number): number;
        static sk_paint_set_blendmode_0(pParams: number, pReturn: number): number;
        static sk_paint_get_blendmode_0(pParams: number, pReturn: number): number;
        static sk_paint_set_filter_quality_0(pParams: number, pReturn: number): number;
        static sk_paint_get_filter_quality_0(pParams: number, pReturn: number): number;
        static sk_paint_get_typeface_0(pParams: number, pReturn: number): number;
        static sk_paint_set_typeface_0(pParams: number, pReturn: number): number;
        static sk_paint_get_textsize_0(pParams: number, pReturn: number): number;
        static sk_paint_set_textsize_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_align_0(pParams: number, pReturn: number): number;
        static sk_paint_set_text_align_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_encoding_0(pParams: number, pReturn: number): number;
        static sk_paint_set_text_encoding_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_scale_x_0(pParams: number, pReturn: number): number;
        static sk_paint_set_text_scale_x_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_skew_x_0(pParams: number, pReturn: number): number;
        static sk_paint_set_text_skew_x_0(pParams: number, pReturn: number): number;
        static sk_paint_measure_text_0(pParams: number, pReturn: number): number;
        static sk_paint_measure_text_1(pParams: number, pReturn: number): number;
        static sk_paint_break_text_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_path_0(pParams: number, pReturn: number): number;
        static sk_paint_get_pos_text_path_0(pParams: number, pReturn: number): number;
        static sk_paint_get_fontmetrics_0(pParams: number, pReturn: number): number;
        static sk_paint_get_fontmetrics_1(pParams: number, pReturn: number): number;
        static sk_paint_get_path_effect_0(pParams: number, pReturn: number): number;
        static sk_paint_set_path_effect_0(pParams: number, pReturn: number): number;
        static sk_paint_is_linear_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_linear_text_0(pParams: number, pReturn: number): number;
        static sk_paint_is_subpixel_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_subpixel_text_0(pParams: number, pReturn: number): number;
        static sk_paint_is_lcd_render_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_lcd_render_text_0(pParams: number, pReturn: number): number;
        static sk_paint_is_embedded_bitmap_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_embedded_bitmap_text_0(pParams: number, pReturn: number): number;
        static sk_paint_is_autohinted_0(pParams: number, pReturn: number): number;
        static sk_paint_set_autohinted_0(pParams: number, pReturn: number): number;
        static sk_paint_get_hinting_0(pParams: number, pReturn: number): number;
        static sk_paint_set_hinting_0(pParams: number, pReturn: number): number;
        static sk_paint_is_fake_bold_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_fake_bold_text_0(pParams: number, pReturn: number): number;
        static sk_paint_is_dev_kern_text_0(pParams: number, pReturn: number): number;
        static sk_paint_set_dev_kern_text_0(pParams: number, pReturn: number): number;
        static sk_paint_get_fill_path_0(pParams: number, pReturn: number): number;
        static sk_paint_get_fill_path_1(pParams: number, pReturn: number): number;
        static sk_paint_clone_0(pParams: number, pReturn: number): number;
        static sk_paint_text_to_glyphs_0(pParams: number, pReturn: number): number;
        static sk_paint_contains_text_0(pParams: number, pReturn: number): number;
        static sk_paint_count_text_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_widths_0(pParams: number, pReturn: number): number;
        static sk_paint_get_text_intercepts_0(pParams: number, pReturn: number): number;
        static sk_paint_get_pos_text_intercepts_0(pParams: number, pReturn: number): number;
        static sk_paint_get_pos_text_h_intercepts_0(pParams: number, pReturn: number): number;
        static sk_paint_get_pos_text_blob_intercepts_0(pParams: number, pReturn: number): number;
        static sk_image_ref_0(pParams: number, pReturn: number): number;
        static sk_image_unref_0(pParams: number, pReturn: number): number;
        static sk_image_new_raster_copy_0(pParams: number, pReturn: number): number;
        static sk_image_new_raster_copy_with_pixmap_0(pParams: number, pReturn: number): number;
        static sk_image_new_raster_data_0(pParams: number, pReturn: number): number;
        static sk_image_new_raster_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_bitmap_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_encoded_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_encoded_1(pParams: number, pReturn: number): number;
        static sk_image_new_from_texture_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_adopted_texture_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_picture_0(pParams: number, pReturn: number): number;
        static sk_image_new_from_picture_1(pParams: number, pReturn: number): number;
        static sk_image_get_width_0(pParams: number, pReturn: number): number;
        static sk_image_get_height_0(pParams: number, pReturn: number): number;
        static sk_image_get_unique_id_0(pParams: number, pReturn: number): number;
        static sk_image_get_alpha_type_0(pParams: number, pReturn: number): number;
        static sk_image_get_color_type_0(pParams: number, pReturn: number): number;
        static sk_image_get_colorspace_0(pParams: number, pReturn: number): number;
        static sk_image_is_alpha_only_0(pParams: number, pReturn: number): number;
        static sk_image_make_shader_0(pParams: number, pReturn: number): number;
        static sk_image_make_shader_1(pParams: number, pReturn: number): number;
        static sk_image_peek_pixels_0(pParams: number, pReturn: number): number;
        static sk_image_is_texture_backed_0(pParams: number, pReturn: number): number;
        static sk_image_is_lazy_generated_0(pParams: number, pReturn: number): number;
        static sk_image_read_pixels_0(pParams: number, pReturn: number): number;
        static sk_image_read_pixels_into_pixmap_0(pParams: number, pReturn: number): number;
        static sk_image_scale_pixels_0(pParams: number, pReturn: number): number;
        static sk_image_ref_encoded_0(pParams: number, pReturn: number): number;
        static sk_image_encode_0(pParams: number, pReturn: number): number;
        static sk_image_encode_specific_0(pParams: number, pReturn: number): number;
        static sk_image_make_subset_0(pParams: number, pReturn: number): number;
        static sk_image_make_non_texture_image_0(pParams: number, pReturn: number): number;
        static sk_image_make_with_filter_0(pParams: number, pReturn: number): number;
        static sk_path_contains_0(pParams: number, pReturn: number): number;
        static sk_path_get_last_point_0(pParams: number, pReturn: number): number;
        static sk_path_new_0(pParams: number, pReturn: number): number;
        static sk_path_delete_0(pParams: number, pReturn: number): number;
        static sk_path_move_to_0(pParams: number, pReturn: number): number;
        static sk_path_rmove_to_0(pParams: number, pReturn: number): number;
        static sk_path_line_to_0(pParams: number, pReturn: number): number;
        static sk_path_rline_to_0(pParams: number, pReturn: number): number;
        static sk_path_quad_to_0(pParams: number, pReturn: number): number;
        static sk_path_rquad_to_0(pParams: number, pReturn: number): number;
        static sk_path_conic_to_0(pParams: number, pReturn: number): number;
        static sk_path_rconic_to_0(pParams: number, pReturn: number): number;
        static sk_path_cubic_to_0(pParams: number, pReturn: number): number;
        static sk_path_rcubic_to_0(pParams: number, pReturn: number): number;
        static sk_path_close_0(pParams: number, pReturn: number): number;
        static sk_path_rewind_0(pParams: number, pReturn: number): number;
        static sk_path_reset_0(pParams: number, pReturn: number): number;
        static sk_path_add_rect_0(pParams: number, pReturn: number): number;
        static sk_path_add_rect_start_0(pParams: number, pReturn: number): number;
        static sk_path_add_rrect_0(pParams: number, pReturn: number): number;
        static sk_path_add_rrect_start_0(pParams: number, pReturn: number): number;
        static sk_path_add_oval_0(pParams: number, pReturn: number): number;
        static sk_path_add_arc_0(pParams: number, pReturn: number): number;
        static sk_path_add_path_offset_0(pParams: number, pReturn: number): number;
        static sk_path_add_path_matrix_0(pParams: number, pReturn: number): number;
        static sk_path_add_path_0(pParams: number, pReturn: number): number;
        static sk_path_add_path_reverse_0(pParams: number, pReturn: number): number;
        static sk_path_get_bounds_0(pParams: number, pReturn: number): number;
        static sk_path_compute_tight_bounds_0(pParams: number, pReturn: number): number;
        static sk_path_get_filltype_0(pParams: number, pReturn: number): number;
        static sk_path_set_filltype_0(pParams: number, pReturn: number): number;
        static sk_path_clone_0(pParams: number, pReturn: number): number;
        static sk_path_transform_0(pParams: number, pReturn: number): number;
        static sk_path_arc_to_0(pParams: number, pReturn: number): number;
        static sk_path_rarc_to_0(pParams: number, pReturn: number): number;
        static sk_path_arc_to_with_oval_0(pParams: number, pReturn: number): number;
        static sk_path_arc_to_with_points_0(pParams: number, pReturn: number): number;
        static sk_path_add_rounded_rect_0(pParams: number, pReturn: number): number;
        static sk_path_add_circle_0(pParams: number, pReturn: number): number;
        static sk_path_count_verbs_0(pParams: number, pReturn: number): number;
        static sk_path_count_points_0(pParams: number, pReturn: number): number;
        static sk_path_get_point_0(pParams: number, pReturn: number): number;
        static sk_path_get_points_0(pParams: number, pReturn: number): number;
        static sk_path_get_convexity_0(pParams: number, pReturn: number): number;
        static sk_path_set_convexity_0(pParams: number, pReturn: number): number;
        static sk_path_parse_svg_string_0(pParams: number, pReturn: number): number;
        static sk_path_to_svg_string_0(pParams: number, pReturn: number): number;
        static sk_path_convert_conic_to_quads_0(pParams: number, pReturn: number): number;
        static sk_path_add_poly_0(pParams: number, pReturn: number): number;
        static sk_path_get_segment_masks_0(pParams: number, pReturn: number): number;
        static sk_path_is_oval_0(pParams: number, pReturn: number): number;
        static sk_path_is_oval_1(pParams: number, pReturn: number): number;
        static sk_path_is_rrect_0(pParams: number, pReturn: number): number;
        static sk_path_is_line_0(pParams: number, pReturn: number): number;
        static sk_path_is_line_1(pParams: number, pReturn: number): number;
        static sk_path_is_rect_0(pParams: number, pReturn: number): number;
        static sk_path_is_rect_1(pParams: number, pReturn: number): number;
        static sk_pathmeasure_new_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_new_with_path_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_destroy_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_set_path_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_length_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_pos_tan_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_pos_tan_1(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_pos_tan_2(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_matrix_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_get_segment_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_is_closed_0(pParams: number, pReturn: number): number;
        static sk_pathmeasure_next_contour_0(pParams: number, pReturn: number): number;
        static sk_pathop_op_0(pParams: number, pReturn: number): number;
        static sk_pathop_simplify_0(pParams: number, pReturn: number): number;
        static sk_pathop_tight_bounds_0(pParams: number, pReturn: number): number;
        static sk_opbuilder_new_0(pParams: number, pReturn: number): number;
        static sk_opbuilder_destroy_0(pParams: number, pReturn: number): number;
        static sk_opbuilder_add_0(pParams: number, pReturn: number): number;
        static sk_opbuilder_resolve_0(pParams: number, pReturn: number): number;
        static sk_path_create_iter_0(pParams: number, pReturn: number): number;
        static sk_path_iter_next_0(pParams: number, pReturn: number): number;
        static sk_path_iter_conic_weight_0(pParams: number, pReturn: number): number;
        static sk_path_iter_is_close_line_0(pParams: number, pReturn: number): number;
        static sk_path_iter_is_closed_contour_0(pParams: number, pReturn: number): number;
        static sk_path_iter_destroy_0(pParams: number, pReturn: number): number;
        static sk_path_create_rawiter_0(pParams: number, pReturn: number): number;
        static sk_path_rawiter_next_0(pParams: number, pReturn: number): number;
        static sk_path_rawiter_peek_0(pParams: number, pReturn: number): number;
        static sk_path_rawiter_conic_weight_0(pParams: number, pReturn: number): number;
        static sk_path_rawiter_destroy_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_unref_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_new_blur_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_new_blur_with_flags_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_new_table_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_new_gamma_0(pParams: number, pReturn: number): number;
        static sk_maskfilter_new_clip_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_croprect_new_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_croprect_new_with_rect_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_croprect_destructor_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_croprect_get_rect_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_croprect_get_flags_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_unref_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_matrix_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_alpha_threshold_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_blur_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_color_filter_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_compose_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_displacement_map_effect_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_drop_shadow_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_distant_lit_diffuse_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_point_lit_diffuse_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_spot_lit_diffuse_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_distant_lit_specular_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_point_lit_specular_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_spot_lit_specular_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_magnifier_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_matrix_convolution_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_merge_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_dilate_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_erode_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_offset_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_picture_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_picture_with_croprect_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_tile_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_xfermode_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_arithmetic_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_image_source_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_image_source_default_0(pParams: number, pReturn: number): number;
        static sk_imagefilter_new_paint_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_unref_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_mode_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_lighting_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_compose_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_color_matrix_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_luma_color_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_table_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_table_argb_0(pParams: number, pReturn: number): number;
        static sk_colorfilter_new_high_contrast_0(pParams: number, pReturn: number): number;
        static sk_data_new_empty_0(pParams: number, pReturn: number): number;
        static sk_data_new_with_copy_0(pParams: number, pReturn: number): number;
        static sk_data_new_with_copy_1(pParams: number, pReturn: number): number;
        static sk_data_new_subset_0(pParams: number, pReturn: number): number;
        static sk_data_new_from_file_0(pParams: number, pReturn: number): number;
        static sk_data_new_from_stream_0(pParams: number, pReturn: number): number;
        static sk_data_new_with_proc_0(pParams: number, pReturn: number): number;
        static sk_data_unref_0(pParams: number, pReturn: number): number;
        static sk_data_get_size_0(pParams: number, pReturn: number): number;
        static sk_data_get_data_0(pParams: number, pReturn: number): number;
        static sk_data_new_uninitialized_0(pParams: number, pReturn: number): number;
        static sk_string_new_empty_0(pParams: number, pReturn: number): number;
        static sk_string_new_with_copy_0(pParams: number, pReturn: number): number;
        static sk_string_destructor_0(pParams: number, pReturn: number): number;
        static sk_string_get_size_0(pParams: number, pReturn: number): number;
        static sk_string_get_c_str_0(pParams: number, pReturn: number): number;
        static sk_picture_recorder_delete_0(pParams: number, pReturn: number): number;
        static sk_picture_recorder_new_0(pParams: number, pReturn: number): number;
        static sk_picture_recorder_begin_recording_0(pParams: number, pReturn: number): number;
        static sk_picture_recorder_end_recording_0(pParams: number, pReturn: number): number;
        static sk_picture_recorder_end_recording_as_drawable_0(pParams: number, pReturn: number): number;
        static sk_picture_get_recording_canvas_0(pParams: number, pReturn: number): number;
        static sk_picture_unref_0(pParams: number, pReturn: number): number;
        static sk_picture_get_unique_id_0(pParams: number, pReturn: number): number;
        static sk_picture_get_cull_rect_0(pParams: number, pReturn: number): number;
        static sk_manageddrawable_new_0(pParams: number, pReturn: number): number;
        static sk_manageddrawable_destroy_0(pParams: number, pReturn: number): number;
        static sk_manageddrawable_set_delegates_0(pParams: number, pReturn: number): number;
        static sk_drawable_get_generation_id_0(pParams: number, pReturn: number): number;
        static sk_drawable_get_bounds_0(pParams: number, pReturn: number): number;
        static sk_drawable_draw_0(pParams: number, pReturn: number): number;
        static sk_drawable_new_picture_snapshot_0(pParams: number, pReturn: number): number;
        static sk_drawable_notify_drawing_changed_0(pParams: number, pReturn: number): number;
        static sk_shader_unref_0(pParams: number, pReturn: number): number;
        static sk_shader_new_empty_0(pParams: number, pReturn: number): number;
        static sk_shader_new_color_0(pParams: number, pReturn: number): number;
        static sk_shader_new_local_matrix_0(pParams: number, pReturn: number): number;
        static sk_shader_new_color_filter_0(pParams: number, pReturn: number): number;
        static sk_shader_new_bitmap_0(pParams: number, pReturn: number): number;
        static sk_shader_new_bitmap_1(pParams: number, pReturn: number): number;
        static sk_shader_new_linear_gradient_0(pParams: number, pReturn: number): number;
        static sk_shader_new_linear_gradient_1(pParams: number, pReturn: number): number;
        static sk_shader_new_linear_gradient_2(pParams: number, pReturn: number): number;
        static sk_shader_new_linear_gradient_3(pParams: number, pReturn: number): number;
        static sk_shader_new_radial_gradient_0(pParams: number, pReturn: number): number;
        static sk_shader_new_radial_gradient_1(pParams: number, pReturn: number): number;
        static sk_shader_new_radial_gradient_2(pParams: number, pReturn: number): number;
        static sk_shader_new_radial_gradient_3(pParams: number, pReturn: number): number;
        static sk_shader_new_sweep_gradient_0(pParams: number, pReturn: number): number;
        static sk_shader_new_sweep_gradient_1(pParams: number, pReturn: number): number;
        static sk_shader_new_sweep_gradient_2(pParams: number, pReturn: number): number;
        static sk_shader_new_sweep_gradient_3(pParams: number, pReturn: number): number;
        static sk_shader_new_two_point_conical_gradient_0(pParams: number, pReturn: number): number;
        static sk_shader_new_two_point_conical_gradient_1(pParams: number, pReturn: number): number;
        static sk_shader_new_two_point_conical_gradient_2(pParams: number, pReturn: number): number;
        static sk_shader_new_two_point_conical_gradient_3(pParams: number, pReturn: number): number;
        static sk_shader_new_perlin_noise_fractal_noise_0(pParams: number, pReturn: number): number;
        static sk_shader_new_perlin_noise_fractal_noise_1(pParams: number, pReturn: number): number;
        static sk_shader_new_perlin_noise_turbulence_0(pParams: number, pReturn: number): number;
        static sk_shader_new_perlin_noise_turbulence_1(pParams: number, pReturn: number): number;
        static sk_shader_new_compose_0(pParams: number, pReturn: number): number;
        static sk_shader_new_compose_with_mode_0(pParams: number, pReturn: number): number;
        static sk_typeface_create_default_0(pParams: number, pReturn: number): number;
        static sk_typeface_ref_default_0(pParams: number, pReturn: number): number;
        static sk_typeface_create_from_name_with_font_style_0(pParams: number, pReturn: number): number;
        static sk_typeface_create_from_file_0(pParams: number, pReturn: number): number;
        static sk_typeface_create_from_stream_0(pParams: number, pReturn: number): number;
        static sk_typeface_unref_0(pParams: number, pReturn: number): number;
        static sk_typeface_chars_to_glyphs_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_family_name_0(pParams: number, pReturn: number): number;
        static sk_typeface_count_tables_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_table_tags_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_table_size_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_table_data_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_fontstyle_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_font_weight_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_font_width_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_font_slant_0(pParams: number, pReturn: number): number;
        static sk_typeface_open_stream_0(pParams: number, pReturn: number): number;
        static sk_typeface_get_units_per_em_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_create_default_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_ref_default_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_unref_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_count_families_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_get_family_name_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_match_family_style_character_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_create_styleset_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_match_family_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_match_family_style_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_match_face_style_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_create_from_data_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_create_from_stream_0(pParams: number, pReturn: number): number;
        static sk_fontmgr_create_from_file_0(pParams: number, pReturn: number): number;
        static sk_fontstyle_new_0(pParams: number, pReturn: number): number;
        static sk_fontstyle_delete_0(pParams: number, pReturn: number): number;
        static sk_fontstyle_get_weight_0(pParams: number, pReturn: number): number;
        static sk_fontstyle_get_width_0(pParams: number, pReturn: number): number;
        static sk_fontstyle_get_slant_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_create_empty_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_unref_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_get_count_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_get_style_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_create_typeface_0(pParams: number, pReturn: number): number;
        static sk_fontstyleset_match_style_0(pParams: number, pReturn: number): number;
        static sk_memorystream_destroy_0(pParams: number, pReturn: number): number;
        static sk_filestream_destroy_0(pParams: number, pReturn: number): number;
        static sk_stream_asset_destroy_0(pParams: number, pReturn: number): number;
        static sk_stream_read_0(pParams: number, pReturn: number): number;
        static sk_stream_peek_0(pParams: number, pReturn: number): number;
        static sk_stream_skip_0(pParams: number, pReturn: number): number;
        static sk_stream_is_at_end_0(pParams: number, pReturn: number): number;
        static sk_stream_read_s8_0(pParams: number, pReturn: number): number;
        static sk_stream_read_s16_0(pParams: number, pReturn: number): number;
        static sk_stream_read_s32_0(pParams: number, pReturn: number): number;
        static sk_stream_read_u8_0(pParams: number, pReturn: number): number;
        static sk_stream_read_u16_0(pParams: number, pReturn: number): number;
        static sk_stream_read_u32_0(pParams: number, pReturn: number): number;
        static sk_stream_read_bool_0(pParams: number, pReturn: number): number;
        static sk_stream_rewind_0(pParams: number, pReturn: number): number;
        static sk_stream_has_position_0(pParams: number, pReturn: number): number;
        static sk_stream_get_position_0(pParams: number, pReturn: number): number;
        static sk_stream_seek_0(pParams: number, pReturn: number): number;
        static sk_stream_move_0(pParams: number, pReturn: number): number;
        static sk_stream_has_length_0(pParams: number, pReturn: number): number;
        static sk_stream_get_length_0(pParams: number, pReturn: number): number;
        static sk_stream_get_memory_base_0(pParams: number, pReturn: number): number;
        static sk_filestream_new_0(pParams: number, pReturn: number): number;
        static sk_memorystream_new_0(pParams: number, pReturn: number): number;
        static sk_memorystream_new_with_length_0(pParams: number, pReturn: number): number;
        static sk_memorystream_new_with_data_0(pParams: number, pReturn: number): number;
        static sk_memorystream_new_with_data_1(pParams: number, pReturn: number): number;
        static sk_memorystream_new_with_skdata_0(pParams: number, pReturn: number): number;
        static sk_memorystream_set_memory_0(pParams: number, pReturn: number): number;
        static sk_memorystream_set_memory_1(pParams: number, pReturn: number): number;
        static sk_filestream_is_valid_0(pParams: number, pReturn: number): number;
        static sk_managedstream_new_0(pParams: number, pReturn: number): number;
        static sk_managedstream_set_delegates_0(pParams: number, pReturn: number): number;
        static sk_managedstream_destroy_0(pParams: number, pReturn: number): number;
        static sk_managedwstream_new_0(pParams: number, pReturn: number): number;
        static sk_managedwstream_destroy_0(pParams: number, pReturn: number): number;
        static sk_managedwstream_set_delegates_0(pParams: number, pReturn: number): number;
        static sk_filewstream_destroy_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_destroy_0(pParams: number, pReturn: number): number;
        static sk_filewstream_new_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_new_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_detach_as_stream_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_detach_as_data_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_copy_to_0(pParams: number, pReturn: number): number;
        static sk_dynamicmemorywstream_write_to_stream_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_1(pParams: number, pReturn: number): number;
        static sk_wstream_newline_0(pParams: number, pReturn: number): number;
        static sk_wstream_flush_0(pParams: number, pReturn: number): number;
        static sk_wstream_bytes_written_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_8_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_16_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_32_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_text_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_dec_as_text_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_bigdec_as_text_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_hex_as_text_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_scalar_as_text_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_bool_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_scalar_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_packed_uint_0(pParams: number, pReturn: number): number;
        static sk_wstream_write_stream_0(pParams: number, pReturn: number): number;
        static sk_wstream_get_size_of_packed_uint_0(pParams: number, pReturn: number): number;
        static sk_filewstream_is_valid_0(pParams: number, pReturn: number): number;
        static sk_document_unref_0(pParams: number, pReturn: number): number;
        static sk_document_create_pdf_from_stream_0(pParams: number, pReturn: number): number;
        static sk_document_create_pdf_from_stream_with_metadata_0(pParams: number, pReturn: number): number;
        static sk_document_create_xps_from_stream_0(pParams: number, pReturn: number): number;
        static sk_document_begin_page_0(pParams: number, pReturn: number): number;
        static sk_document_begin_page_1(pParams: number, pReturn: number): number;
        static sk_document_end_page_0(pParams: number, pReturn: number): number;
        static sk_document_close_0(pParams: number, pReturn: number): number;
        static sk_document_abort_0(pParams: number, pReturn: number): number;
        static sk_codec_min_buffered_bytes_needed_0(pParams: number, pReturn: number): number;
        static sk_codec_new_from_stream_0(pParams: number, pReturn: number): number;
        static sk_codec_new_from_data_0(pParams: number, pReturn: number): number;
        static sk_codec_destroy_0(pParams: number, pReturn: number): number;
        static sk_codec_get_info_0(pParams: number, pReturn: number): number;
        static sk_codec_get_origin_0(pParams: number, pReturn: number): number;
        static sk_codec_get_scaled_dimensions_0(pParams: number, pReturn: number): number;
        static sk_codec_get_valid_subset_0(pParams: number, pReturn: number): number;
        static sk_codec_get_encoded_format_0(pParams: number, pReturn: number): number;
        static sk_codec_get_pixels_0(pParams: number, pReturn: number): number;
        static sk_codec_start_incremental_decode_0(pParams: number, pReturn: number): number;
        static sk_codec_start_incremental_decode_1(pParams: number, pReturn: number): number;
        static sk_codec_incremental_decode_0(pParams: number, pReturn: number): number;
        static sk_codec_get_repetition_count_0(pParams: number, pReturn: number): number;
        static sk_codec_get_frame_count_0(pParams: number, pReturn: number): number;
        static sk_codec_get_frame_info_0(pParams: number, pReturn: number): number;
        static sk_codec_get_frame_info_for_index_0(pParams: number, pReturn: number): number;
        static sk_codec_start_scanline_decode_0(pParams: number, pReturn: number): number;
        static sk_codec_start_scanline_decode_1(pParams: number, pReturn: number): number;
        static sk_codec_get_scanlines_0(pParams: number, pReturn: number): number;
        static sk_codec_skip_scanlines_0(pParams: number, pReturn: number): number;
        static sk_codec_get_scanline_order_0(pParams: number, pReturn: number): number;
        static sk_codec_next_scanline_0(pParams: number, pReturn: number): number;
        static sk_codec_output_scanline_0(pParams: number, pReturn: number): number;
        static sk_bitmap_new_0(pParams: number, pReturn: number): number;
        static sk_bitmap_destructor_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_info_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_pixel_colors_0(pParams: number, pReturn: number): number;
        static sk_bitmap_set_pixel_colors_0(pParams: number, pReturn: number): number;
        static sk_bitmap_reset_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_row_bytes_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_byte_count_0(pParams: number, pReturn: number): number;
        static sk_bitmap_is_null_0(pParams: number, pReturn: number): number;
        static sk_bitmap_is_immutable_0(pParams: number, pReturn: number): number;
        static sk_bitmap_set_immutable_0(pParams: number, pReturn: number): number;
        static sk_bitmap_is_volatile_0(pParams: number, pReturn: number): number;
        static sk_bitmap_set_volatile_0(pParams: number, pReturn: number): number;
        static sk_bitmap_erase_0(pParams: number, pReturn: number): number;
        static sk_bitmap_erase_rect_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_addr_8_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_addr_16_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_addr_32_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_addr_0(pParams: number, pReturn: number): number;
        static sk_bitmap_get_pixel_color_0(pParams: number, pReturn: number): number;
        static sk_bitmap_set_pixel_color_0(pParams: number, pReturn: number): number;
        static sk_bitmap_ready_to_draw_0(pParams: number, pReturn: number): number;
        static sk_bitmap_install_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_install_pixels_with_pixmap_0(pParams: number, pReturn: number): number;
        static sk_bitmap_install_mask_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_try_alloc_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_try_alloc_pixels_with_flags_0(pParams: number, pReturn: number): number;
        static sk_bitmap_set_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_peek_pixels_0(pParams: number, pReturn: number): number;
        static sk_bitmap_extract_subset_0(pParams: number, pReturn: number): number;
        static sk_bitmap_extract_alpha_0(pParams: number, pReturn: number): number;
        static sk_bitmap_notify_pixels_changed_0(pParams: number, pReturn: number): number;
        static sk_bitmap_swap_0(pParams: number, pReturn: number): number;
        static sk_color_unpremultiply_0(pParams: number, pReturn: number): number;
        static sk_color_premultiply_0(pParams: number, pReturn: number): number;
        static sk_color_unpremultiply_array_0(pParams: number, pReturn: number): number;
        static sk_color_premultiply_array_0(pParams: number, pReturn: number): number;
        static sk_color_get_bit_shift_0(pParams: number, pReturn: number): number;
        static sk_pixmap_destructor_0(pParams: number, pReturn: number): number;
        static sk_pixmap_new_0(pParams: number, pReturn: number): number;
        static sk_pixmap_new_with_params_0(pParams: number, pReturn: number): number;
        static sk_pixmap_reset_0(pParams: number, pReturn: number): number;
        static sk_pixmap_reset_with_params_0(pParams: number, pReturn: number): number;
        static sk_pixmap_get_info_0(pParams: number, pReturn: number): number;
        static sk_pixmap_get_row_bytes_0(pParams: number, pReturn: number): number;
        static sk_pixmap_get_pixels_0(pParams: number, pReturn: number): number;
        static sk_pixmap_get_pixels_with_xy_0(pParams: number, pReturn: number): number;
        static sk_pixmap_get_pixel_color_0(pParams: number, pReturn: number): number;
        static sk_pixmap_extract_subset_0(pParams: number, pReturn: number): number;
        static sk_pixmap_erase_color_0(pParams: number, pReturn: number): number;
        static sk_pixmap_encode_image_0(pParams: number, pReturn: number): number;
        static sk_pixmap_read_pixels_0(pParams: number, pReturn: number): number;
        static sk_pixmap_scale_pixels_0(pParams: number, pReturn: number): number;
        static sk_swizzle_swap_rb_0(pParams: number, pReturn: number): number;
        static sk_webpencoder_encode_0(pParams: number, pReturn: number): number;
        static sk_jpegencoder_encode_0(pParams: number, pReturn: number): number;
        static sk_pngencoder_encode_0(pParams: number, pReturn: number): number;
        static sk_mask_alloc_image_0(pParams: number, pReturn: number): number;
        static sk_mask_free_image_0(pParams: number, pReturn: number): number;
        static sk_mask_is_empty_0(pParams: number, pReturn: number): number;
        static sk_mask_compute_image_size_0(pParams: number, pReturn: number): number;
        static sk_mask_compute_total_image_size_0(pParams: number, pReturn: number): number;
        static sk_mask_get_addr_1_0(pParams: number, pReturn: number): number;
        static sk_mask_get_addr_8_0(pParams: number, pReturn: number): number;
        static sk_mask_get_addr_lcd_16_0(pParams: number, pReturn: number): number;
        static sk_mask_get_addr_32_0(pParams: number, pReturn: number): number;
        static sk_mask_get_addr_0(pParams: number, pReturn: number): number;
        static sk_matrix_try_invert_0(pParams: number, pReturn: number): number;
        static sk_matrix_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix_pre_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix_post_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_rect_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_points_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_vectors_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_xy_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_vector_0(pParams: number, pReturn: number): number;
        static sk_matrix_map_radius_0(pParams: number, pReturn: number): number;
        static sk_3dview_new_0(pParams: number, pReturn: number): number;
        static sk_3dview_destroy_0(pParams: number, pReturn: number): number;
        static sk_3dview_save_0(pParams: number, pReturn: number): number;
        static sk_3dview_restore_0(pParams: number, pReturn: number): number;
        static sk_3dview_translate_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_x_degrees_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_y_degrees_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_z_degrees_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_x_radians_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_y_radians_0(pParams: number, pReturn: number): number;
        static sk_3dview_rotate_z_radians_0(pParams: number, pReturn: number): number;
        static sk_3dview_get_matrix_0(pParams: number, pReturn: number): number;
        static sk_3dview_apply_to_canvas_0(pParams: number, pReturn: number): number;
        static sk_3dview_dot_with_normal_0(pParams: number, pReturn: number): number;
        static sk_matrix44_destroy_0(pParams: number, pReturn: number): number;
        static sk_matrix44_new_0(pParams: number, pReturn: number): number;
        static sk_matrix44_new_identity_0(pParams: number, pReturn: number): number;
        static sk_matrix44_new_copy_0(pParams: number, pReturn: number): number;
        static sk_matrix44_new_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix44_new_matrix_0(pParams: number, pReturn: number): number;
        static sk_matrix44_equals_0(pParams: number, pReturn: number): number;
        static sk_matrix44_to_matrix_0(pParams: number, pReturn: number): number;
        static sk_matrix44_get_type_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_identity_0(pParams: number, pReturn: number): number;
        static sk_matrix44_get_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_0(pParams: number, pReturn: number): number;
        static sk_matrix44_as_col_major_0(pParams: number, pReturn: number): number;
        static sk_matrix44_as_row_major_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_col_major_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_row_major_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_translate_0(pParams: number, pReturn: number): number;
        static sk_matrix44_pre_translate_0(pParams: number, pReturn: number): number;
        static sk_matrix44_post_translate_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_scale_0(pParams: number, pReturn: number): number;
        static sk_matrix44_pre_scale_0(pParams: number, pReturn: number): number;
        static sk_matrix44_post_scale_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_rotate_about_degrees_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_rotate_about_radians_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_rotate_about_radians_unit_0(pParams: number, pReturn: number): number;
        static sk_matrix44_set_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix44_pre_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix44_post_concat_0(pParams: number, pReturn: number): number;
        static sk_matrix44_invert_0(pParams: number, pReturn: number): number;
        static sk_matrix44_transpose_0(pParams: number, pReturn: number): number;
        static sk_matrix44_map_scalars_0(pParams: number, pReturn: number): number;
        static sk_matrix44_map2_0(pParams: number, pReturn: number): number;
        static sk_matrix44_preserves_2d_axis_alignment_0(pParams: number, pReturn: number): number;
        static sk_matrix44_determinant_0(pParams: number, pReturn: number): number;
        static sk_path_effect_unref_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_compose_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_sum_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_discrete_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_corner_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_1d_path_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_2d_line_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_2d_path_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_dash_0(pParams: number, pReturn: number): number;
        static sk_path_effect_create_trim_0(pParams: number, pReturn: number): number;
        static sk_colortable_unref_0(pParams: number, pReturn: number): number;
        static sk_colortable_new_0(pParams: number, pReturn: number): number;
        static sk_colortable_count_0(pParams: number, pReturn: number): number;
        static sk_colortable_read_colors_0(pParams: number, pReturn: number): number;
        static gr_context_make_gl_0(pParams: number, pReturn: number): number;
        static gr_context_unref_0(pParams: number, pReturn: number): number;
        static gr_context_abandon_context_0(pParams: number, pReturn: number): number;
        static gr_context_release_resources_and_abandon_context_0(pParams: number, pReturn: number): number;
        static gr_context_get_resource_cache_limits_0(pParams: number, pReturn: number): number;
        static gr_context_set_resource_cache_limits_0(pParams: number, pReturn: number): number;
        static gr_context_get_resource_cache_usage_0(pParams: number, pReturn: number): number;
        static gr_context_get_max_surface_sample_count_for_color_type_0(pParams: number, pReturn: number): number;
        static gr_context_flush_0(pParams: number, pReturn: number): number;
        static gr_context_reset_context_0(pParams: number, pReturn: number): number;
        static gr_context_get_backend_0(pParams: number, pReturn: number): number;
        static gr_glinterface_assemble_interface_0(pParams: number, pReturn: number): number;
        static gr_glinterface_assemble_gl_interface_0(pParams: number, pReturn: number): number;
        static gr_glinterface_assemble_gles_interface_0(pParams: number, pReturn: number): number;
        static gr_glinterface_create_native_interface_0(pParams: number, pReturn: number): number;
        static gr_glinterface_unref_0(pParams: number, pReturn: number): number;
        static gr_glinterface_validate_0(pParams: number, pReturn: number): number;
        static gr_glinterface_has_extension_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_new_gl_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_delete_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_is_valid_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_get_width_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_get_height_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_has_mipmaps_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_get_backend_0(pParams: number, pReturn: number): number;
        static gr_backendtexture_get_gl_textureinfo_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_new_gl_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_delete_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_is_valid_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_width_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_height_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_samples_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_stencils_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_backend_0(pParams: number, pReturn: number): number;
        static gr_backendrendertarget_get_gl_framebufferinfo_0(pParams: number, pReturn: number): number;
        static sk_xmlstreamwriter_new_0(pParams: number, pReturn: number): number;
        static sk_xmlstreamwriter_delete_0(pParams: number, pReturn: number): number;
        static sk_svgcanvas_create_0(pParams: number, pReturn: number): number;
        static sk_region_new_0(pParams: number, pReturn: number): number;
        static sk_region_new2_0(pParams: number, pReturn: number): number;
        static sk_region_contains_0(pParams: number, pReturn: number): number;
        static sk_region_contains2_0(pParams: number, pReturn: number): number;
        static sk_region_intersects_0(pParams: number, pReturn: number): number;
        static sk_region_intersects_rect_0(pParams: number, pReturn: number): number;
        static sk_region_set_region_0(pParams: number, pReturn: number): number;
        static sk_region_set_rect_0(pParams: number, pReturn: number): number;
        static sk_region_set_path_0(pParams: number, pReturn: number): number;
        static sk_region_op_0(pParams: number, pReturn: number): number;
        static sk_region_op2_0(pParams: number, pReturn: number): number;
        static sk_region_get_bounds_0(pParams: number, pReturn: number): number;
        static sk_vertices_unref_0(pParams: number, pReturn: number): number;
        static sk_vertices_make_copy_0(pParams: number, pReturn: number): number;
        static sk_rrect_new_0(pParams: number, pReturn: number): number;
        static sk_rrect_new_copy_0(pParams: number, pReturn: number): number;
        static sk_rrect_delete_0(pParams: number, pReturn: number): number;
        static sk_rrect_get_type_0(pParams: number, pReturn: number): number;
        static sk_rrect_get_rect_0(pParams: number, pReturn: number): number;
        static sk_rrect_get_radii_0(pParams: number, pReturn: number): number;
        static sk_rrect_get_width_0(pParams: number, pReturn: number): number;
        static sk_rrect_get_height_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_empty_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_rect_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_oval_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_rect_xy_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_nine_patch_0(pParams: number, pReturn: number): number;
        static sk_rrect_set_rect_radii_0(pParams: number, pReturn: number): number;
        static sk_rrect_inset_0(pParams: number, pReturn: number): number;
        static sk_rrect_outset_0(pParams: number, pReturn: number): number;
        static sk_rrect_offset_0(pParams: number, pReturn: number): number;
        static sk_rrect_contains_0(pParams: number, pReturn: number): number;
        static sk_rrect_is_valid_0(pParams: number, pReturn: number): number;
        static sk_rrect_transform_0(pParams: number, pReturn: number): number;
        static sk_textblob_ref_0(pParams: number, pReturn: number): number;
        static sk_textblob_unref_0(pParams: number, pReturn: number): number;
        static sk_textblob_get_unique_id_0(pParams: number, pReturn: number): number;
        static sk_textblob_get_bounds_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_new_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_delete_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_make_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_alloc_run_text_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_alloc_run_text_pos_h_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_alloc_run_text_pos_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_runbuffer_set_glyphs_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_runbuffer_set_pos_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_runbuffer_set_pos_points_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_runbuffer_set_utf8_text_0(pParams: number, pReturn: number): number;
        static sk_textblob_builder_runbuffer_set_clusters_0(pParams: number, pReturn: number): number;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_delete_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_backend_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_backend_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_gl_framebufferinfo_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_gl_framebufferinfo_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_gl_framebufferinfo_0_Return {
        glInfo: SkiaSharp.GRGlFramebufferInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_height_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_height_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_samples_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_samples_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_stencils_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_stencils_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_get_width_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_get_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_is_valid_0_Params {
        rendertarget: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_is_valid_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_new_gl_0_Params {
        width: number;
        height: number;
        samples: number;
        stencils: number;
        glInfo: SkiaSharp.GRGlFramebufferInfo;
        static unmarshal(pData: number, memoryContext?: any): gr_backendrendertarget_new_gl_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendrendertarget_new_gl_0_Return {
        glInfo: SkiaSharp.GRGlFramebufferInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_delete_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_get_backend_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_get_backend_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_get_gl_textureinfo_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_get_gl_textureinfo_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_get_gl_textureinfo_0_Return {
        glInfo: SkiaSharp.GRGlTextureInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_get_height_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_get_height_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_get_width_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_get_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_has_mipmaps_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_has_mipmaps_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_is_valid_0_Params {
        texture: number;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_is_valid_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_new_gl_0_Params {
        width: number;
        height: number;
        mipmapped: boolean;
        glInfo: SkiaSharp.GRGlTextureInfo;
        static unmarshal(pData: number, memoryContext?: any): gr_backendtexture_new_gl_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_backendtexture_new_gl_0_Return {
        glInfo: SkiaSharp.GRGlTextureInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_context_abandon_context_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_abandon_context_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_flush_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_flush_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_backend_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_get_backend_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_max_surface_sample_count_for_color_type_0_Params {
        context: number;
        colorType: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_get_max_surface_sample_count_for_color_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_resource_cache_limits_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_get_resource_cache_limits_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_resource_cache_limits_0_Return {
        maxResources: number;
        maxResourceBytes: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_resource_cache_usage_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_get_resource_cache_usage_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_get_resource_cache_usage_0_Return {
        maxResources: number;
        maxResourceBytes: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class gr_context_make_gl_0_Params {
        glInterface: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_make_gl_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_release_resources_and_abandon_context_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_release_resources_and_abandon_context_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_reset_context_0_Params {
        context: number;
        state: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_reset_context_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_set_resource_cache_limits_0_Params {
        context: number;
        maxResources: number;
        maxResourceBytes: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_set_resource_cache_limits_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_context_unref_0_Params {
        context: number;
        static unmarshal(pData: number, memoryContext?: any): gr_context_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_assemble_gl_interface_0_Params {
        ctx: number;
        get: number;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_assemble_gl_interface_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_assemble_gles_interface_0_Params {
        ctx: number;
        get: number;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_assemble_gles_interface_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_assemble_interface_0_Params {
        ctx: number;
        get: number;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_assemble_interface_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_has_extension_0_Params {
        glInterface: number;
        extension: string;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_has_extension_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_unref_0_Params {
        glInterface: number;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class gr_glinterface_validate_0_Params {
        glInterface: number;
        static unmarshal(pData: number, memoryContext?: any): gr_glinterface_validate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_apply_to_canvas_0_Params {
        cview: number;
        ccanvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_apply_to_canvas_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_destroy_0_Params {
        cview: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_dot_with_normal_0_Params {
        cview: number;
        dx: number;
        dy: number;
        dz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_dot_with_normal_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_get_matrix_0_Params {
        cview: number;
        cmatrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_get_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_get_matrix_0_Return {
        cmatrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_restore_0_Params {
        cview: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_restore_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_x_degrees_0_Params {
        cview: number;
        degrees: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_x_degrees_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_x_radians_0_Params {
        cview: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_x_radians_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_y_degrees_0_Params {
        cview: number;
        degrees: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_y_degrees_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_y_radians_0_Params {
        cview: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_y_radians_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_z_degrees_0_Params {
        cview: number;
        degrees: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_z_degrees_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_rotate_z_radians_0_Params {
        cview: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_rotate_z_radians_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_save_0_Params {
        cview: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_save_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_3dview_translate_0_Params {
        cview: number;
        x: number;
        y: number;
        z: number;
        static unmarshal(pData: number, memoryContext?: any): sk_3dview_translate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_destructor_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_destructor_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_erase_0_Params {
        cbitmap: number;
        color: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_erase_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_erase_rect_0_Params {
        cbitmap: number;
        color: SkiaSharp.SKColor;
        rect: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_erase_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_erase_rect_0_Return {
        rect: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_extract_alpha_0_Params {
        cbitmap: number;
        dst: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_extract_alpha_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_extract_alpha_0_Return {
        offset: SkiaSharp.SKPointI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_extract_subset_0_Params {
        cbitmap: number;
        cdst: number;
        subset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_extract_subset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_extract_subset_0_Return {
        subset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_addr_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_addr_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_addr_16_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_addr_16_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_addr_32_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_addr_32_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_addr_8_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_addr_8_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_byte_count_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_byte_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_info_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_info_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_info_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_pixel_color_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_pixel_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_pixel_colors_0_Params {
        b: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_pixel_colors_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_pixels_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_pixels_0_Return {
        length: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_get_row_bytes_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_get_row_bytes_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_install_mask_pixels_0_Params {
        cbitmap: number;
        cmask: SkiaSharp.SKMask;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_install_mask_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_install_mask_pixels_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_install_pixels_0_Params {
        cbitmap: number;
        cinfo: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        releaseProc: number;
        context: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_install_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_install_pixels_0_Return {
        cinfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_install_pixels_with_pixmap_0_Params {
        cbitmap: number;
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_install_pixels_with_pixmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_is_immutable_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_is_immutable_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_is_null_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_is_null_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_is_volatile_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_is_volatile_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_notify_pixels_changed_0_Params {
        cbitmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_notify_pixels_changed_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_peek_pixels_0_Params {
        cbitmap: number;
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_peek_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_ready_to_draw_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_ready_to_draw_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_reset_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_reset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_set_immutable_0_Params {
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_set_immutable_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_set_pixel_color_0_Params {
        cbitmap: number;
        x: number;
        y: number;
        color: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_set_pixel_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_set_pixel_colors_0_Params {
        b: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_set_pixel_colors_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_set_pixels_0_Params {
        cbitmap: number;
        pixels: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_set_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_set_volatile_0_Params {
        b: number;
        value: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_set_volatile_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_swap_0_Params {
        cbitmap: number;
        cother: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_swap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_try_alloc_pixels_0_Params {
        cbitmap: number;
        requestedInfo: SkiaSharp.SKImageInfoNative;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_try_alloc_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_try_alloc_pixels_0_Return {
        requestedInfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_try_alloc_pixels_with_flags_0_Params {
        cbitmap: number;
        requestedInfo: SkiaSharp.SKImageInfoNative;
        flags: number;
        static unmarshal(pData: number, memoryContext?: any): sk_bitmap_try_alloc_pixels_with_flags_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_bitmap_try_alloc_pixels_with_flags_0_Return {
        requestedInfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_clip_path_with_operation_0_Params {
        t: number;
        cpath: number;
        op: number;
        doAA: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_clip_path_with_operation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_clip_rect_with_operation_0_Params {
        t: number;
        crect: SkiaSharp.SKRect;
        op: number;
        doAA: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_clip_rect_with_operation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_clip_rect_with_operation_0_Return {
        crect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_clip_region_0_Params {
        t: number;
        region: number;
        op: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_clip_region_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_clip_rrect_with_operation_0_Params {
        t: number;
        crect: number;
        op: number;
        doAA: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_clip_rrect_with_operation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_concat_0_Params {
        t: number;
        m: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_concat_0_Return {
        m: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_destroy_0_Params {
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_annotation_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        key_Length: number;
        key: Array<number>;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_annotation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_annotation_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_0_Params {
        t: number;
        bitmap: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_bitmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_lattice_0_Params {
        t: number;
        bitmap: number;
        lattice: SkiaSharp.SKLatticeInternal;
        dst: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_bitmap_lattice_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_lattice_0_Return {
        lattice: SkiaSharp.SKLatticeInternal;
        dst: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_nine_0_Params {
        t: number;
        bitmap: number;
        center: SkiaSharp.SKRectI;
        dst: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_bitmap_nine_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_nine_0_Return {
        center: SkiaSharp.SKRectI;
        dst: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_rect_0_Params {
        t: number;
        bitmap: number;
        src: SkiaSharp.SKRect;
        dest: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_bitmap_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_rect_0_Return {
        src: SkiaSharp.SKRect;
        dest: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_rect_1_Params {
        t: number;
        bitmap: number;
        srcZero: number;
        dest: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_bitmap_rect_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_bitmap_rect_1_Return {
        dest: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_circle_0_Params {
        t: number;
        cx: number;
        cy: number;
        radius: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_circle_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_color_0_Params {
        t: number;
        color: SkiaSharp.SKColor;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_drawable_0_Params {
        t: number;
        drawable: number;
        mat: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_drawable_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_drawable_0_Return {
        mat: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_0_Params {
        t: number;
        image: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_image_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_lattice_0_Params {
        t: number;
        image: number;
        lattice: SkiaSharp.SKLatticeInternal;
        dst: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_image_lattice_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_lattice_0_Return {
        lattice: SkiaSharp.SKLatticeInternal;
        dst: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_nine_0_Params {
        t: number;
        image: number;
        center: SkiaSharp.SKRectI;
        dst: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_image_nine_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_nine_0_Return {
        center: SkiaSharp.SKRectI;
        dst: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_rect_0_Params {
        t: number;
        image: number;
        src: SkiaSharp.SKRect;
        dest: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_image_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_rect_0_Return {
        src: SkiaSharp.SKRect;
        dest: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_rect_1_Params {
        t: number;
        image: number;
        srcZero: number;
        dest: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_image_rect_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_image_rect_1_Return {
        dest: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_line_0_Params {
        t: number;
        x0: number;
        y0: number;
        x1: number;
        y1: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_line_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_link_destination_annotation_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_link_destination_annotation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_link_destination_annotation_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_named_destination_annotation_0_Params {
        t: number;
        point: SkiaSharp.SKPoint;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_named_destination_annotation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_named_destination_annotation_0_Return {
        point: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_oval_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_oval_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_oval_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_paint_0_Params {
        t: number;
        p: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_paint_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_path_0_Params {
        t: number;
        path: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_picture_0_Params {
        t: number;
        pict: number;
        mat: SkiaSharp.SKMatrix;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_picture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_picture_0_Return {
        mat: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_picture_1_Params {
        t: number;
        pict: number;
        matZero: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_picture_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_point_0_Params {
        t: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_point_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_points_0_Params {
        t: number;
        mode: number;
        count: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_pos_text_0_Params {
        t: number;
        text_Length: number;
        text: Array<number>;
        len: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_pos_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_pos_text_1_Params {
        t: number;
        text: number;
        len: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_pos_text_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_rect_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_region_0_Params {
        t: number;
        region: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_region_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_round_rect_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        rx: number;
        ry: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_round_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_round_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_rrect_0_Params {
        t: number;
        rect: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_rrect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_text_0_Params {
        t: number;
        text_Length: number;
        text: Array<number>;
        len: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_text_1_Params {
        t: number;
        text: number;
        len: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_text_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_text_blob_0_Params {
        canvas: number;
        text: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_text_blob_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_text_on_path_0_Params {
        t: number;
        text_Length: number;
        text: Array<number>;
        len: number;
        path: number;
        hOffset: number;
        vOffset: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_text_on_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_text_on_path_1_Params {
        t: number;
        text: number;
        len: number;
        path: number;
        hOffset: number;
        vOffset: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_text_on_path_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_url_annotation_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_url_annotation_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_url_annotation_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_draw_vertices_0_Params {
        canvas: number;
        vertices: number;
        mode: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_draw_vertices_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_flush_0_Params {
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_flush_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_device_clip_bounds_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_get_device_clip_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_device_clip_bounds_0_Return {
        cbounds: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_local_clip_bounds_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_get_local_clip_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_local_clip_bounds_0_Return {
        cbounds: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_save_count_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_get_save_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_total_matrix_0_Params {
        canvas: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_get_total_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_get_total_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_new_from_bitmap_0_Params {
        bitmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_new_from_bitmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_quick_reject_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_quick_reject_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_quick_reject_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_reset_matrix_0_Params {
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_reset_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_restore_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_restore_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_restore_to_count_0_Params {
        t: number;
        saveCount: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_restore_to_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_rotate_degrees_0_Params {
        t: number;
        degrees: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_rotate_degrees_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_rotate_radians_0_Params {
        t: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_rotate_radians_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_save_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_save_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_save_layer_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_save_layer_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_save_layer_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_save_layer_1_Params {
        t: number;
        rectZero: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_save_layer_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_scale_0_Params {
        t: number;
        sx: number;
        sy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_scale_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_set_matrix_0_Params {
        canvas: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_set_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_set_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_skew_0_Params {
        t: number;
        sx: number;
        sy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_skew_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_canvas_translate_0_Params {
        t: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_canvas_translate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_destroy_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_encoded_format_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_encoded_format_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_frame_count_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_frame_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_frame_info_0_Params {
        codec: number;
        frameInfo_Length: number;
        frameInfo: Array<SkiaSharp.SKCodecFrameInfo>;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_frame_info_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_frame_info_for_index_0_Params {
        codec: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_frame_info_for_index_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_frame_info_for_index_0_Return {
        frameInfo: SkiaSharp.SKCodecFrameInfo;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_info_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_info_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_info_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_origin_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_origin_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_pixels_0_Params {
        codec: number;
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        options: SkiaSharp.SKCodecOptionsInternal;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_pixels_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        options: SkiaSharp.SKCodecOptionsInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_repetition_count_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_repetition_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_scaled_dimensions_0_Params {
        codec: number;
        desiredScale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_scaled_dimensions_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_scaled_dimensions_0_Return {
        dimensions: SkiaSharp.SKSizeI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_scanline_order_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_scanline_order_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_scanlines_0_Params {
        codec: number;
        dst: number;
        countLines: number;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_scanlines_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_valid_subset_0_Params {
        codec: number;
        desiredSubset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_get_valid_subset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_get_valid_subset_0_Return {
        desiredSubset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_incremental_decode_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_incremental_decode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_incremental_decode_0_Return {
        rowsDecoded: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_new_from_data_0_Params {
        data: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_new_from_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_new_from_stream_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_new_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_new_from_stream_0_Return {
        result: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_next_scanline_0_Params {
        codec: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_next_scanline_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_output_scanline_0_Params {
        codec: number;
        inputScanline: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_output_scanline_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_skip_scanlines_0_Params {
        codec: number;
        countLines: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_skip_scanlines_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_incremental_decode_0_Params {
        codec: number;
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        options: SkiaSharp.SKCodecOptionsInternal;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_start_incremental_decode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_incremental_decode_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        options: SkiaSharp.SKCodecOptionsInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_incremental_decode_1_Params {
        codec: number;
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        optionsZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_start_incremental_decode_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_incremental_decode_1_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_scanline_decode_0_Params {
        codec: number;
        info: SkiaSharp.SKImageInfoNative;
        options: SkiaSharp.SKCodecOptionsInternal;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_start_scanline_decode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_scanline_decode_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        options: SkiaSharp.SKCodecOptionsInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_scanline_decode_1_Params {
        codec: number;
        info: SkiaSharp.SKImageInfoNative;
        optionsZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_codec_start_scanline_decode_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_codec_start_scanline_decode_1_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_color_get_bit_shift_0_Params {
        static unmarshal(pData: number, memoryContext?: any): sk_color_get_bit_shift_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_color_get_bit_shift_0_Return {
        a: number;
        r: number;
        g: number;
        b: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_color_premultiply_0_Params {
        color: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_color_premultiply_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_color_premultiply_array_0_Params {
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        size: number;
        pmcolors_Length: number;
        pmcolors: Array<SkiaSharp.SKPMColor>;
        static unmarshal(pData: number, memoryContext?: any): sk_color_premultiply_array_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_color_unpremultiply_0_Params {
        pmcolor: SkiaSharp.SKPMColor;
        static unmarshal(pData: number, memoryContext?: any): sk_color_unpremultiply_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_color_unpremultiply_array_0_Params {
        pmcolors_Length: number;
        pmcolors: Array<SkiaSharp.SKPMColor>;
        size: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        static unmarshal(pData: number, memoryContext?: any): sk_color_unpremultiply_array_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_color_matrix_0_Params {
        array_Length: number;
        array: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_color_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_compose_0_Params {
        outer: number;
        inner: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_compose_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_high_contrast_0_Params {
        config: SkiaSharp.SKHighContrastConfig;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_high_contrast_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_high_contrast_0_Return {
        config: SkiaSharp.SKHighContrastConfig;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_lighting_0_Params {
        mul: SkiaSharp.SKColor;
        add: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_lighting_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_mode_0_Params {
        c: SkiaSharp.SKColor;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_mode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_table_0_Params {
        table_Length: number;
        table: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_table_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_new_table_argb_0_Params {
        tableA_Length: number;
        tableA: Array<number>;
        tableR_Length: number;
        tableR: Array<number>;
        tableG_Length: number;
        tableG: Array<number>;
        tableB_Length: number;
        tableB: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_new_table_argb_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorfilter_unref_0_Params {
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorfilter_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_as_from_xyzd50_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_as_from_xyzd50_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_as_to_xyzd50_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_as_to_xyzd50_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_equals_0_Params {
        src: number;
        dst: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_equals_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_gamma_close_to_srgb_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_gamma_close_to_srgb_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_gamma_get_gamma_named_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_gamma_get_gamma_named_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_gamma_get_type_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_gamma_get_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_gamma_is_linear_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_gamma_is_linear_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_is_numerical_transfer_fn_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_is_numerical_transfer_fn_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_is_numerical_transfer_fn_0_Return {
        fn: SkiaSharp.SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_is_srgb_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_is_srgb_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_icc_0_Params {
        input: number;
        len: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_icc_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_icc_1_Params {
        input_Length: number;
        input: Array<number>;
        len: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_icc_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_coeffs_0_Params {
        coeffs: SkiaSharp.SKColorSpaceTransferFn;
        toXYZD50: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_coeffs_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_coeffs_0_Return {
        coeffs: SkiaSharp.SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Params {
        coeffs: SkiaSharp.SKColorSpaceTransferFn;
        gamut: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_coeffs_and_gamut_0_Return {
        coeffs: SkiaSharp.SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_gamma_0_Params {
        gamma: number;
        toXYZD50: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_gamma_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_gamma_and_gamut_0_Params {
        gamma: number;
        gamut: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_gamma_and_gamut_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_gamma_named_0_Params {
        gamma: number;
        toXYZD50: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_gamma_named_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Params {
        gamma: number;
        gamut: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_new_rgb_with_gamma_named_and_gamut_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_to_xyzd50_0_Params {
        cColorSpace: number;
        toXYZD50: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_to_xyzd50_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_transfer_fn_invert_0_Params {
        transfer: SkiaSharp.SKColorSpaceTransferFn;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_transfer_fn_invert_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_transfer_fn_invert_0_Return {
        transfer: SkiaSharp.SKColorSpaceTransferFn;
        inverted: SkiaSharp.SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_transfer_fn_transform_0_Params {
        transfer: SkiaSharp.SKColorSpaceTransferFn;
        x: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_transfer_fn_transform_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_transfer_fn_transform_0_Return {
        transfer: SkiaSharp.SKColorSpaceTransferFn;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colorspace_unref_0_Params {
        cColorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspace_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspaceprimaries_to_xyzd50_0_Params {
        primaries: SkiaSharp.SKColorSpacePrimaries;
        toXYZD50: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colorspaceprimaries_to_xyzd50_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colorspaceprimaries_to_xyzd50_0_Return {
        primaries: SkiaSharp.SKColorSpacePrimaries;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colortable_count_0_Params {
        ctable: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colortable_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colortable_new_0_Params {
        colors_Length: number;
        colors: Array<SkiaSharp.SKPMColor>;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colortable_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colortable_read_colors_0_Params {
        ctable: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colortable_read_colors_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_colortable_read_colors_0_Return {
        colors: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_colortable_unref_0_Params {
        ctable: number;
        static unmarshal(pData: number, memoryContext?: any): sk_colortable_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_get_data_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_get_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_get_size_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_get_size_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_from_file_0_Params {
        utf8path_Length: number;
        utf8path: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_from_file_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_from_stream_0_Params {
        stream: number;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_subset_0_Params {
        src: number;
        offset: number;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_subset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_uninitialized_0_Params {
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_uninitialized_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_with_copy_0_Params {
        src: number;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_with_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_with_copy_1_Params {
        src_Length: number;
        src: Array<number>;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_with_copy_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_new_with_proc_0_Params {
        ptr: number;
        length: number;
        proc: number;
        ctx: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_new_with_proc_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_data_unref_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_data_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_abort_0_Params {
        document: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_abort_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_begin_page_0_Params {
        document: number;
        width: number;
        height: number;
        content: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_document_begin_page_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_begin_page_0_Return {
        content: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_document_begin_page_1_Params {
        document: number;
        width: number;
        height: number;
        contentZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_begin_page_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_close_0_Params {
        document: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_close_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_create_pdf_from_stream_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_create_pdf_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_create_pdf_from_stream_with_metadata_0_Params {
        stream: number;
        metadata: SkiaSharp.SKDocumentPdfMetadataInternal;
        static unmarshal(pData: number, memoryContext?: any): sk_document_create_pdf_from_stream_with_metadata_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_create_pdf_from_stream_with_metadata_0_Return {
        metadata: SkiaSharp.SKDocumentPdfMetadataInternal;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_document_create_xps_from_stream_0_Params {
        stream: number;
        dpi: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_create_xps_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_end_page_0_Params {
        document: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_end_page_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_document_unref_0_Params {
        document: number;
        static unmarshal(pData: number, memoryContext?: any): sk_document_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_draw_0_Params {
        d: number;
        c: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_drawable_draw_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_draw_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_get_bounds_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_drawable_get_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_get_bounds_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_get_generation_id_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_drawable_get_generation_id_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_new_picture_snapshot_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_drawable_new_picture_snapshot_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_drawable_notify_drawing_changed_0_Params {
        d: number;
        static unmarshal(pData: number, memoryContext?: any): sk_drawable_notify_drawing_changed_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_dynamicmemorywstream_copy_to_0_Params {
        cstream: number;
        data: number;
        static unmarshal(pData: number, memoryContext?: any): sk_dynamicmemorywstream_copy_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_dynamicmemorywstream_destroy_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_dynamicmemorywstream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_dynamicmemorywstream_detach_as_data_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_dynamicmemorywstream_detach_as_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_dynamicmemorywstream_detach_as_stream_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_dynamicmemorywstream_detach_as_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_dynamicmemorywstream_write_to_stream_0_Params {
        cstream: number;
        dst: number;
        static unmarshal(pData: number, memoryContext?: any): sk_dynamicmemorywstream_write_to_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filestream_destroy_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_filestream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filestream_is_valid_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_filestream_is_valid_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filestream_new_0_Params {
        utf8path_Length: number;
        utf8path: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_filestream_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filewstream_destroy_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_filewstream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filewstream_is_valid_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_filewstream_is_valid_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_filewstream_new_0_Params {
        utf8path_Length: number;
        utf8path: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_filewstream_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_count_families_0_Params {
        fontmgr: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_count_families_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_create_from_data_0_Params {
        fontmgr: number;
        data: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_create_from_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_create_from_file_0_Params {
        fontmgr: number;
        utf8path_Length: number;
        utf8path: Array<number>;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_create_from_file_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_create_from_stream_0_Params {
        fontmgr: number;
        stream: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_create_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_create_styleset_0_Params {
        fontmgr: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_create_styleset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_get_family_name_0_Params {
        fontmgr: number;
        index: number;
        familyName: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_get_family_name_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_match_face_style_0_Params {
        fontmgr: number;
        face: number;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_match_face_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_match_family_0_Params {
        fontmgr: number;
        familyName: string;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_match_family_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_match_family_style_0_Params {
        fontmgr: number;
        familyName: string;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_match_family_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_match_family_style_character_0_Params {
        fontmgr: number;
        familyName: string;
        style: number;
        bcp47_Length: number;
        bcp47: Array<string>;
        bcp47Count: number;
        character: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_match_family_style_character_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontmgr_unref_0_Params {
        fontmgr: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontmgr_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyle_delete_0_Params {
        fs: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyle_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyle_get_slant_0_Params {
        fs: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyle_get_slant_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyle_get_weight_0_Params {
        fs: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyle_get_weight_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyle_get_width_0_Params {
        fs: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyle_get_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyle_new_0_Params {
        weight: number;
        width: number;
        slant: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyle_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyleset_create_typeface_0_Params {
        fss: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyleset_create_typeface_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyleset_get_count_0_Params {
        fss: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyleset_get_count_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyleset_get_style_0_Params {
        fss: number;
        index: number;
        fs: number;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyleset_get_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyleset_match_style_0_Params {
        fss: number;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyleset_match_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_fontstyleset_unref_0_Params {
        fss: number;
        static unmarshal(pData: number, memoryContext?: any): sk_fontstyleset_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_encode_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_encode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_encode_specific_0_Params {
        image: number;
        encoder: number;
        quality: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_encode_specific_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_alpha_type_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_alpha_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_color_type_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_color_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_colorspace_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_colorspace_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_height_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_height_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_unique_id_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_unique_id_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_get_width_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_get_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_is_alpha_only_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_is_alpha_only_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_is_lazy_generated_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_is_lazy_generated_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_is_texture_backed_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_is_texture_backed_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_non_texture_image_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_make_non_texture_image_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_shader_0_Params {
        image: number;
        tileX: number;
        tileY: number;
        localMatrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_image_make_shader_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_shader_0_Return {
        localMatrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_shader_1_Params {
        image: number;
        tileX: number;
        tileY: number;
        localMatrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_make_shader_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_subset_0_Params {
        image: number;
        subset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_image_make_subset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_subset_0_Return {
        subset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_with_filter_0_Params {
        image: number;
        filter: number;
        subset: SkiaSharp.SKRectI;
        clipbounds: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_image_make_with_filter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_make_with_filter_0_Return {
        subset: SkiaSharp.SKRectI;
        clipbounds: SkiaSharp.SKRectI;
        outSubset: SkiaSharp.SKRectI;
        outOffset: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_adopted_texture_0_Params {
        context: number;
        texture: number;
        origin: number;
        colorType: number;
        alpha: number;
        colorSpace: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_adopted_texture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_bitmap_0_Params {
        cbitmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_bitmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_encoded_0_Params {
        encoded: number;
        subset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_encoded_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_encoded_0_Return {
        subset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_encoded_1_Params {
        encoded: number;
        subsetZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_encoded_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_picture_0_Params {
        picture: number;
        dimensions: SkiaSharp.SKSizeI;
        matrix: SkiaSharp.SKMatrix;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_picture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_picture_0_Return {
        dimensions: SkiaSharp.SKSizeI;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_picture_1_Params {
        picture: number;
        dimensions: SkiaSharp.SKSizeI;
        matrixZero: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_picture_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_picture_1_Return {
        dimensions: SkiaSharp.SKSizeI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_from_texture_0_Params {
        context: number;
        texture: number;
        origin: number;
        colorType: number;
        alpha: number;
        colorSpace: number;
        releaseProc: number;
        releaseContext: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_from_texture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_0_Params {
        pixmap: number;
        releaseProc: number;
        context: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_raster_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_copy_0_Params {
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_raster_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_copy_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_copy_with_pixmap_0_Params {
        pixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_raster_copy_with_pixmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_data_0_Params {
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_new_raster_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_new_raster_data_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_peek_pixels_0_Params {
        image: number;
        pixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_peek_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_read_pixels_0_Params {
        image: number;
        dstInfo: SkiaSharp.SKImageInfoNative;
        dstPixels: number;
        dstRowBytes: number;
        srcX: number;
        srcY: number;
        cachingHint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_read_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_read_pixels_0_Return {
        dstInfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_image_read_pixels_into_pixmap_0_Params {
        image: number;
        dst: number;
        srcX: number;
        srcY: number;
        cachingHint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_read_pixels_into_pixmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_ref_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_ref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_ref_encoded_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_ref_encoded_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_scale_pixels_0_Params {
        image: number;
        dst: number;
        quality: number;
        cachingHint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_scale_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_image_unref_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_image_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_destructor_0_Params {
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_croprect_destructor_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_get_flags_0_Params {
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_croprect_get_flags_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_get_rect_0_Params {
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_croprect_get_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_get_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_new_with_rect_0_Params {
        rect: SkiaSharp.SKRect;
        flags: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_croprect_new_with_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_croprect_new_with_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_alpha_threshold_0_Params {
        region: number;
        innerThreshold: number;
        outerThreshold: number;
        input: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_alpha_threshold_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_arithmetic_0_Params {
        k1: number;
        k2: number;
        k3: number;
        k4: number;
        enforcePMColor: boolean;
        background: number;
        foreground: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_arithmetic_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_blur_0_Params {
        sigmaX: number;
        sigmaY: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_blur_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_color_filter_0_Params {
        cf: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_color_filter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_compose_0_Params {
        outer: number;
        inner: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_compose_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_dilate_0_Params {
        radiusX: number;
        radiusY: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_dilate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_displacement_map_effect_0_Params {
        xChannelSelector: number;
        yChannelSelector: number;
        scale: number;
        displacement: number;
        color: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_displacement_map_effect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_distant_lit_diffuse_0_Params {
        direction: SkiaSharp.SKPoint3;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        kd: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_distant_lit_diffuse_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_distant_lit_diffuse_0_Return {
        direction: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_distant_lit_specular_0_Params {
        direction: SkiaSharp.SKPoint3;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        ks: number;
        shininess: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_distant_lit_specular_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_distant_lit_specular_0_Return {
        direction: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_drop_shadow_0_Params {
        dx: number;
        dy: number;
        sigmaX: number;
        sigmaY: number;
        color: SkiaSharp.SKColor;
        shadowMode: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_drop_shadow_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_erode_0_Params {
        radiusX: number;
        radiusY: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_erode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_image_source_0_Params {
        image: number;
        srcRect: SkiaSharp.SKRect;
        dstRect: SkiaSharp.SKRect;
        filterQuality: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_image_source_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_image_source_0_Return {
        srcRect: SkiaSharp.SKRect;
        dstRect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_image_source_default_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_image_source_default_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_magnifier_0_Params {
        src: SkiaSharp.SKRect;
        inset: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_magnifier_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_magnifier_0_Return {
        src: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_matrix_0_Params {
        matrix: SkiaSharp.SKMatrix;
        quality: number;
        input: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_matrix_convolution_0_Params {
        kernelSize: SkiaSharp.SKSizeI;
        kernel_Length: number;
        kernel: Array<number>;
        gain: number;
        bias: number;
        kernelOffset: SkiaSharp.SKPointI;
        tileMode: number;
        convolveAlpha: boolean;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_matrix_convolution_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_matrix_convolution_0_Return {
        kernelSize: SkiaSharp.SKSizeI;
        kernelOffset: SkiaSharp.SKPointI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_merge_0_Params {
        filters_Length: number;
        filters: Array<number>;
        count: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_merge_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_offset_0_Params {
        dx: number;
        dy: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_offset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_paint_0_Params {
        paint: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_paint_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_picture_0_Params {
        picture: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_picture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_picture_with_croprect_0_Params {
        picture: number;
        cropRect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_picture_with_croprect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_picture_with_croprect_0_Return {
        cropRect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_point_lit_diffuse_0_Params {
        location: SkiaSharp.SKPoint3;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        kd: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_point_lit_diffuse_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_point_lit_diffuse_0_Return {
        location: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_point_lit_specular_0_Params {
        location: SkiaSharp.SKPoint3;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        ks: number;
        shininess: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_point_lit_specular_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_point_lit_specular_0_Return {
        location: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_spot_lit_diffuse_0_Params {
        location: SkiaSharp.SKPoint3;
        target: SkiaSharp.SKPoint3;
        specularExponent: number;
        cutoffAngle: number;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        kd: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_spot_lit_diffuse_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_spot_lit_diffuse_0_Return {
        location: SkiaSharp.SKPoint3;
        target: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_spot_lit_specular_0_Params {
        location: SkiaSharp.SKPoint3;
        target: SkiaSharp.SKPoint3;
        specularExponent: number;
        cutoffAngle: number;
        lightColor: SkiaSharp.SKColor;
        surfaceScale: number;
        ks: number;
        shininess: number;
        input: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_spot_lit_specular_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_spot_lit_specular_0_Return {
        location: SkiaSharp.SKPoint3;
        target: SkiaSharp.SKPoint3;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_tile_0_Params {
        src: SkiaSharp.SKRect;
        dst: SkiaSharp.SKRect;
        input: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_tile_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_tile_0_Return {
        src: SkiaSharp.SKRect;
        dst: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_new_xfermode_0_Params {
        mode: number;
        background: number;
        foreground: number;
        cropRect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_new_xfermode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_imagefilter_unref_0_Params {
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_imagefilter_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_jpegencoder_encode_0_Params {
        dst: number;
        src: number;
        options: SkiaSharp.SKJpegEncoderOptions;
        static unmarshal(pData: number, memoryContext?: any): sk_jpegencoder_encode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_manageddrawable_destroy_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_manageddrawable_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_manageddrawable_set_delegates_0_Params {
        pDraw: number;
        pGetBounds: number;
        pNewPictureSnapshot: number;
        static unmarshal(pData: number, memoryContext?: any): sk_manageddrawable_set_delegates_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_managedstream_destroy_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_managedstream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_managedstream_set_delegates_0_Params {
        pRead: number;
        pPeek: number;
        pIsAtEnd: number;
        pHasPosition: number;
        pHasLength: number;
        pRewind: number;
        pGetPosition: number;
        pSeek: number;
        pMove: number;
        pGetLength: number;
        pCreateNew: number;
        pDestroy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_managedstream_set_delegates_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_managedwstream_destroy_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_managedwstream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_managedwstream_set_delegates_0_Params {
        pWrite: number;
        pFlush: number;
        pBytesWritten: number;
        pDestroy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_managedwstream_set_delegates_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_alloc_image_0_Params {
        bytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_alloc_image_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_compute_image_size_0_Params {
        cmask: SkiaSharp.SKMask;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_compute_image_size_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_compute_image_size_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_compute_total_image_size_0_Params {
        cmask: SkiaSharp.SKMask;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_compute_total_image_size_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_compute_total_image_size_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_free_image_0_Params {
        image: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_free_image_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_0_Params {
        cmask: SkiaSharp.SKMask;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_get_addr_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_1_0_Params {
        cmask: SkiaSharp.SKMask;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_get_addr_1_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_1_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_32_0_Params {
        cmask: SkiaSharp.SKMask;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_get_addr_32_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_32_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_8_0_Params {
        cmask: SkiaSharp.SKMask;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_get_addr_8_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_8_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_lcd_16_0_Params {
        cmask: SkiaSharp.SKMask;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_get_addr_lcd_16_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_get_addr_lcd_16_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_mask_is_empty_0_Params {
        cmask: SkiaSharp.SKMask;
        static unmarshal(pData: number, memoryContext?: any): sk_mask_is_empty_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_mask_is_empty_0_Return {
        cmask: SkiaSharp.SKMask;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_blur_0_Params {
        style: number;
        sigma: number;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_new_blur_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_blur_with_flags_0_Params {
        style: number;
        sigma: number;
        occluder: SkiaSharp.SKRect;
        respectCTM: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_new_blur_with_flags_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_blur_with_flags_0_Return {
        occluder: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_clip_0_Params {
        min: number;
        max: number;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_new_clip_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_gamma_0_Params {
        gamma: number;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_new_gamma_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_new_table_0_Params {
        table_Length: number;
        table: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_new_table_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_maskfilter_unref_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_maskfilter_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_as_col_major_0_Params {
        matrix: number;
        dst_Length: number;
        dst: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_as_col_major_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_as_row_major_0_Params {
        matrix: number;
        dst_Length: number;
        dst: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_as_row_major_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_destroy_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_determinant_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_determinant_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_equals_0_Params {
        matrix: number;
        other: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_equals_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_get_0_Params {
        matrix: number;
        row: number;
        col: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_get_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_get_type_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_get_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_invert_0_Params {
        matrix: number;
        inverse: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_invert_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_map2_0_Params {
        matrix: number;
        src2_Length: number;
        src2: Array<number>;
        count: number;
        dst_Length: number;
        dst: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_map2_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_map_scalars_0_Params {
        matrix: number;
        src_Length: number;
        src: Array<number>;
        dst_Length: number;
        dst: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_map_scalars_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_new_concat_0_Params {
        a: number;
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_new_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_new_copy_0_Params {
        src: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_new_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_new_matrix_0_Params {
        src: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_new_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_new_matrix_0_Return {
        src: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_post_concat_0_Params {
        matrix: number;
        m: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_post_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_post_scale_0_Params {
        matrix: number;
        sx: number;
        sy: number;
        sz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_post_scale_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_post_translate_0_Params {
        matrix: number;
        dx: number;
        dy: number;
        dz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_post_translate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_pre_concat_0_Params {
        matrix: number;
        m: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_pre_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_pre_scale_0_Params {
        matrix: number;
        sx: number;
        sy: number;
        sz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_pre_scale_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_pre_translate_0_Params {
        matrix: number;
        dx: number;
        dy: number;
        dz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_pre_translate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_preserves_2d_axis_alignment_0_Params {
        matrix: number;
        epsilon: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_preserves_2d_axis_alignment_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_0_Params {
        matrix: number;
        row: number;
        col: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_col_major_0_Params {
        matrix: number;
        src_Length: number;
        src: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_col_major_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_concat_0_Params {
        matrix: number;
        a: number;
        b: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_identity_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_identity_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_rotate_about_degrees_0_Params {
        matrix: number;
        x: number;
        y: number;
        z: number;
        degrees: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_rotate_about_degrees_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_rotate_about_radians_0_Params {
        matrix: number;
        x: number;
        y: number;
        z: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_rotate_about_radians_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_rotate_about_radians_unit_0_Params {
        matrix: number;
        x: number;
        y: number;
        z: number;
        radians: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_rotate_about_radians_unit_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_row_major_0_Params {
        matrix: number;
        src_Length: number;
        src: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_row_major_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_scale_0_Params {
        matrix: number;
        sx: number;
        sy: number;
        sz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_scale_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_set_translate_0_Params {
        matrix: number;
        dx: number;
        dy: number;
        dz: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_set_translate_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_to_matrix_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_to_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_to_matrix_0_Return {
        dst: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix44_transpose_0_Params {
        matrix: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix44_transpose_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_concat_0_Params {
        target: SkiaSharp.SKMatrix;
        first: SkiaSharp.SKMatrix;
        second: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_concat_0_Return {
        target: SkiaSharp.SKMatrix;
        first: SkiaSharp.SKMatrix;
        second: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_points_0_Params {
        matrix: SkiaSharp.SKMatrix;
        dst: number;
        src: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_points_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_radius_0_Params {
        matrix: SkiaSharp.SKMatrix;
        radius: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_radius_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_radius_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_rect_0_Params {
        matrix: SkiaSharp.SKMatrix;
        source: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_rect_0_Return {
        matrix: SkiaSharp.SKMatrix;
        dest: SkiaSharp.SKRect;
        source: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_vector_0_Params {
        matrix: SkiaSharp.SKMatrix;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_vector_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_vector_0_Return {
        matrix: SkiaSharp.SKMatrix;
        result: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_vectors_0_Params {
        matrix: SkiaSharp.SKMatrix;
        dst: number;
        src: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_vectors_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_vectors_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_xy_0_Params {
        matrix: SkiaSharp.SKMatrix;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_map_xy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_map_xy_0_Return {
        matrix: SkiaSharp.SKMatrix;
        result: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_post_concat_0_Params {
        target: SkiaSharp.SKMatrix;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_post_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_post_concat_0_Return {
        target: SkiaSharp.SKMatrix;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_pre_concat_0_Params {
        target: SkiaSharp.SKMatrix;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_pre_concat_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_pre_concat_0_Return {
        target: SkiaSharp.SKMatrix;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_try_invert_0_Params {
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_matrix_try_invert_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_matrix_try_invert_0_Return {
        matrix: SkiaSharp.SKMatrix;
        result: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_destroy_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_new_with_data_0_Params {
        data: number;
        length: number;
        copyData: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_new_with_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_new_with_data_1_Params {
        data_Length: number;
        data: Array<number>;
        length: number;
        copyData: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_new_with_data_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_new_with_length_0_Params {
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_new_with_length_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_new_with_skdata_0_Params {
        data: number;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_new_with_skdata_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_set_memory_0_Params {
        s: number;
        data: number;
        length: number;
        copyData: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_set_memory_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_memorystream_set_memory_1_Params {
        s: number;
        data_Length: number;
        data: Array<number>;
        length: number;
        copyData: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_memorystream_set_memory_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nodraw_canvas_destroy_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nodraw_canvas_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nodraw_canvas_new_0_Params {
        width: number;
        height: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nodraw_canvas_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nway_canvas_add_canvas_0_Params {
        t: number;
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nway_canvas_add_canvas_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nway_canvas_destroy_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nway_canvas_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nway_canvas_new_0_Params {
        width: number;
        height: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nway_canvas_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nway_canvas_remove_all_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nway_canvas_remove_all_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_nway_canvas_remove_canvas_0_Params {
        t: number;
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_nway_canvas_remove_canvas_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_opbuilder_add_0_Params {
        builder: number;
        path: number;
        op: number;
        static unmarshal(pData: number, memoryContext?: any): sk_opbuilder_add_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_opbuilder_destroy_0_Params {
        builder: number;
        static unmarshal(pData: number, memoryContext?: any): sk_opbuilder_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_opbuilder_resolve_0_Params {
        builder: number;
        result: number;
        static unmarshal(pData: number, memoryContext?: any): sk_opbuilder_resolve_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_overdraw_canvas_destroy_0_Params {
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_overdraw_canvas_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_overdraw_canvas_new_0_Params {
        canvas: number;
        static unmarshal(pData: number, memoryContext?: any): sk_overdraw_canvas_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_break_text_0_Params {
        t: number;
        text: number;
        length: number;
        maxWidth: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_break_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_break_text_0_Return {
        measuredWidth: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_paint_clone_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_clone_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_contains_text_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_contains_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_count_text_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_count_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_delete_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_blendmode_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_blendmode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_color_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_colorfilter_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_colorfilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fill_path_0_Params {
        paint: number;
        src: number;
        dst: number;
        cullRect: SkiaSharp.SKRect;
        resScale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_fill_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fill_path_0_Return {
        cullRect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fill_path_1_Params {
        paint: number;
        src: number;
        dst: number;
        cullRectZero: number;
        resScale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_fill_path_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_filter_quality_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_filter_quality_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fontmetrics_0_Params {
        t: number;
        scale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_fontmetrics_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fontmetrics_0_Return {
        fontMetrics: SkiaSharp.SKFontMetrics;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_fontmetrics_1_Params {
        t: number;
        fontMetricsZero: number;
        scale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_fontmetrics_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_hinting_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_hinting_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_imagefilter_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_imagefilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_maskfilter_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_maskfilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_path_effect_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_path_effect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_pos_text_blob_intercepts_0_Params {
        cpaint: number;
        blob: number;
        bounds_Length: number;
        bounds: Array<number>;
        intervals: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_pos_text_blob_intercepts_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_pos_text_h_intercepts_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        xpos_Length: number;
        xpos: Array<number>;
        y: number;
        bounds_Length: number;
        bounds: Array<number>;
        intervals: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_pos_text_h_intercepts_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_pos_text_intercepts_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        pos_Length: number;
        pos: Array<SkiaSharp.SKPoint>;
        bounds_Length: number;
        bounds: Array<number>;
        intervals: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_pos_text_intercepts_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_pos_text_path_0_Params {
        t: number;
        text: number;
        length: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_pos_text_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_shader_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_shader_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_stroke_cap_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_stroke_cap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_stroke_join_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_stroke_join_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_stroke_miter_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_stroke_miter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_stroke_width_0_Params {
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_stroke_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_style_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_align_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_align_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_encoding_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_encoding_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_intercepts_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        x: number;
        y: number;
        bounds_Length: number;
        bounds: Array<number>;
        intervals: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_intercepts_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_path_0_Params {
        t: number;
        text: number;
        length: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_scale_x_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_scale_x_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_skew_x_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_skew_x_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_text_widths_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        widths: number;
        bounds: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_text_widths_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_textsize_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_textsize_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_get_typeface_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_get_typeface_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_antialias_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_antialias_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_autohinted_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_autohinted_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_dev_kern_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_dev_kern_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_dither_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_dither_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_embedded_bitmap_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_embedded_bitmap_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_fake_bold_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_fake_bold_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_lcd_render_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_lcd_render_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_linear_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_linear_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_subpixel_text_0_Params {
        cpaint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_subpixel_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_is_verticaltext_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_is_verticaltext_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_measure_text_0_Params {
        t: number;
        text: number;
        length: number;
        bounds: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_measure_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_measure_text_0_Return {
        bounds: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_paint_measure_text_1_Params {
        t: number;
        text: number;
        length: number;
        boundsZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_measure_text_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_reset_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_reset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_antialias_0_Params {
        t: number;
        v: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_antialias_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_autohinted_0_Params {
        cpaint: number;
        useAutohinter: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_autohinted_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_blendmode_0_Params {
        t: number;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_blendmode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_color_0_Params {
        t: number;
        color: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_colorfilter_0_Params {
        t: number;
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_colorfilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_dev_kern_text_0_Params {
        cpaint: number;
        devKernText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_dev_kern_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_dither_0_Params {
        t: number;
        v: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_dither_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_embedded_bitmap_text_0_Params {
        cpaint: number;
        useEmbeddedBitmapText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_embedded_bitmap_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_fake_bold_text_0_Params {
        cpaint: number;
        fakeBoldText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_fake_bold_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_filter_quality_0_Params {
        t: number;
        filterQuality: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_filter_quality_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_hinting_0_Params {
        cpaint: number;
        hintingLevel: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_hinting_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_imagefilter_0_Params {
        t: number;
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_imagefilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_lcd_render_text_0_Params {
        cpaint: number;
        lcdText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_lcd_render_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_linear_text_0_Params {
        cpaint: number;
        linearText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_linear_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_maskfilter_0_Params {
        t: number;
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_maskfilter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_path_effect_0_Params {
        cpaint: number;
        effect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_path_effect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_shader_0_Params {
        t: number;
        shader: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_shader_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_stroke_cap_0_Params {
        t: number;
        cap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_stroke_cap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_stroke_join_0_Params {
        t: number;
        join: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_stroke_join_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_stroke_miter_0_Params {
        t: number;
        miter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_stroke_miter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_stroke_width_0_Params {
        t: number;
        width: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_stroke_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_style_0_Params {
        t: number;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_subpixel_text_0_Params {
        cpaint: number;
        subpixelText: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_subpixel_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_text_align_0_Params {
        t: number;
        align: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_text_align_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_text_encoding_0_Params {
        t: number;
        encoding: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_text_encoding_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_text_scale_x_0_Params {
        t: number;
        scale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_text_scale_x_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_text_skew_x_0_Params {
        t: number;
        skew: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_text_skew_x_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_textsize_0_Params {
        t: number;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_textsize_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_typeface_0_Params {
        t: number;
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_typeface_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_set_verticaltext_0_Params {
        t: number;
        v: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_set_verticaltext_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_paint_text_to_glyphs_0_Params {
        cpaint: number;
        text: number;
        byteLength: number;
        glyphs: number;
        static unmarshal(pData: number, memoryContext?: any): sk_paint_text_to_glyphs_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_arc_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        startAngle: number;
        sweepAngle: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_arc_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_arc_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_circle_0_Params {
        t: number;
        x: number;
        y: number;
        radius: number;
        dir: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_circle_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_oval_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        direction: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_oval_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_oval_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_path_0_Params {
        t: number;
        other: number;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_path_matrix_0_Params {
        t: number;
        other: number;
        matrix: SkiaSharp.SKMatrix;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_path_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_path_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_path_offset_0_Params {
        t: number;
        other: number;
        dx: number;
        dy: number;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_path_offset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_path_reverse_0_Params {
        t: number;
        other: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_path_reverse_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_poly_0_Params {
        cpath: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        count: number;
        close: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_poly_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rect_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        direction: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rect_start_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        direction: number;
        startIndex: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_rect_start_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rect_start_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rounded_rect_0_Params {
        t: number;
        rect: SkiaSharp.SKRect;
        rx: number;
        ry: number;
        dir: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_rounded_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rounded_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rrect_0_Params {
        t: number;
        rect: number;
        direction: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_rrect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_add_rrect_start_0_Params {
        t: number;
        rect: number;
        direction: number;
        startIndex: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_add_rrect_start_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_arc_to_0_Params {
        t: number;
        rx: number;
        ry: number;
        xAxisRotate: number;
        largeArc: number;
        sweep: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_arc_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_arc_to_with_oval_0_Params {
        t: number;
        oval: SkiaSharp.SKRect;
        startAngle: number;
        sweepAngle: number;
        forceMoveTo: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_path_arc_to_with_oval_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_arc_to_with_oval_0_Return {
        oval: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_arc_to_with_points_0_Params {
        t: number;
        x1: number;
        y1: number;
        x2: number;
        y2: number;
        radius: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_arc_to_with_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_clone_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_clone_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_close_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_close_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_compute_tight_bounds_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_compute_tight_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_compute_tight_bounds_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_conic_to_0_Params {
        t: number;
        x0: number;
        y0: number;
        x1: number;
        y1: number;
        w: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_conic_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_contains_0_Params {
        cpath: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_contains_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_convert_conic_to_quads_0_Params {
        p0: SkiaSharp.SKPoint;
        p1: SkiaSharp.SKPoint;
        p2: SkiaSharp.SKPoint;
        w: number;
        pts_Length: number;
        pts: Array<SkiaSharp.SKPoint>;
        pow2: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_convert_conic_to_quads_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_convert_conic_to_quads_0_Return {
        p0: SkiaSharp.SKPoint;
        p1: SkiaSharp.SKPoint;
        p2: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_count_points_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_count_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_count_verbs_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_count_verbs_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_create_iter_0_Params {
        path: number;
        forceClose: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_create_iter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_create_rawiter_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_create_rawiter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_cubic_to_0_Params {
        t: number;
        x0: number;
        y0: number;
        x1: number;
        y1: number;
        x2: number;
        y2: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_cubic_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_delete_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_1d_path_0_Params {
        path: number;
        advance: number;
        phase: number;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_1d_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_2d_line_0_Params {
        width: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_2d_line_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_2d_line_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_2d_path_0_Params {
        matrix: SkiaSharp.SKMatrix;
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_2d_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_2d_path_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_compose_0_Params {
        outer: number;
        inner: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_compose_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_corner_0_Params {
        radius: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_corner_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_dash_0_Params {
        intervals_Length: number;
        intervals: Array<number>;
        count: number;
        phase: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_dash_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_discrete_0_Params {
        segLength: number;
        deviation: number;
        seedAssist: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_discrete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_sum_0_Params {
        first: number;
        second: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_sum_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_create_trim_0_Params {
        start: number;
        stop: number;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_create_trim_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_effect_unref_0_Params {
        effect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_effect_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_bounds_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_bounds_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_convexity_0_Params {
        cpath: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_convexity_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_filltype_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_filltype_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_last_point_0_Params {
        cpath: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_last_point_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_last_point_0_Return {
        point: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_point_0_Params {
        path: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_point_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_point_0_Return {
        point: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_points_0_Params {
        path: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        max: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_get_segment_masks_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_get_segment_masks_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_line_0_Params {
        cpath: number;
        line_Length: number;
        line: Array<SkiaSharp.SKPoint>;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_line_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_line_1_Params {
        cpath: number;
        lineZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_line_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_oval_0_Params {
        cpath: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_oval_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_oval_0_Return {
        bounds: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_oval_1_Params {
        cpath: number;
        boundsZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_oval_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_rect_0_Params {
        cpath: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_rect_0_Return {
        rect: SkiaSharp.SKRect;
        isClosed: boolean;
        direction: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_rect_1_Params {
        cpath: number;
        rectZero: number;
        isClosedZero: number;
        directionZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_rect_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_is_rrect_0_Params {
        cpath: number;
        bounds: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_is_rrect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_iter_conic_weight_0_Params {
        iterator: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_iter_conic_weight_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_iter_destroy_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_iter_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_iter_is_close_line_0_Params {
        iterator: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_iter_is_close_line_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_iter_is_closed_contour_0_Params {
        iterator: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_iter_is_closed_contour_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_iter_next_0_Params {
        iterator: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        doConsumeDegenerates: number;
        exact: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_iter_next_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_line_to_0_Params {
        t: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_line_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_move_to_0_Params {
        t: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_move_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_parse_svg_string_0_Params {
        cpath: number;
        str: string;
        static unmarshal(pData: number, memoryContext?: any): sk_path_parse_svg_string_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_quad_to_0_Params {
        t: number;
        x0: number;
        y0: number;
        x1: number;
        y1: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_quad_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rarc_to_0_Params {
        t: number;
        rx: number;
        ry: number;
        xAxisRotate: number;
        largeArc: number;
        sweep: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rarc_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rawiter_conic_weight_0_Params {
        iterator: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rawiter_conic_weight_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rawiter_destroy_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rawiter_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rawiter_next_0_Params {
        iterator: number;
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rawiter_next_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rawiter_peek_0_Params {
        iterator: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rawiter_peek_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rconic_to_0_Params {
        t: number;
        dx0: number;
        dy0: number;
        dx1: number;
        dy1: number;
        w: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rconic_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rcubic_to_0_Params {
        t: number;
        dx0: number;
        dy0: number;
        dx1: number;
        dy1: number;
        dx2: number;
        dy2: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rcubic_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_reset_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_reset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rewind_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rewind_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rline_to_0_Params {
        t: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rline_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rmove_to_0_Params {
        t: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rmove_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_rquad_to_0_Params {
        t: number;
        dx0: number;
        dy0: number;
        dx1: number;
        dy1: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_rquad_to_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_set_convexity_0_Params {
        cpath: number;
        convexity: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_set_convexity_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_set_filltype_0_Params {
        t: number;
        filltype: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_set_filltype_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_to_svg_string_0_Params {
        cpath: number;
        str: number;
        static unmarshal(pData: number, memoryContext?: any): sk_path_to_svg_string_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_transform_0_Params {
        t: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_path_transform_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_path_transform_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_destroy_0_Params {
        pathMeasure: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_length_0_Params {
        pathMeasure: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_length_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_matrix_0_Params {
        pathMeasure: number;
        distance: number;
        flags: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_0_Params {
        pathMeasure: number;
        distance: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_pos_tan_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_0_Return {
        position: SkiaSharp.SKPoint;
        tangent: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_1_Params {
        pathMeasure: number;
        distance: number;
        positionZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_pos_tan_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_1_Return {
        tangent: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_2_Params {
        pathMeasure: number;
        distance: number;
        tangentZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_pos_tan_2_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_pos_tan_2_Return {
        position: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_get_segment_0_Params {
        pathMeasure: number;
        start: number;
        stop: number;
        dst: number;
        startWithMoveTo: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_get_segment_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_is_closed_0_Params {
        pathMeasure: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_is_closed_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_new_with_path_0_Params {
        path: number;
        forceClosed: boolean;
        resScale: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_new_with_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_next_contour_0_Params {
        pathMeasure: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_next_contour_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathmeasure_set_path_0_Params {
        pathMeasure: number;
        path: number;
        forceClosed: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_pathmeasure_set_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathop_op_0_Params {
        one: number;
        two: number;
        op: number;
        result: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathop_op_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathop_simplify_0_Params {
        path: number;
        result: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathop_simplify_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathop_tight_bounds_0_Params {
        path: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pathop_tight_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pathop_tight_bounds_0_Return {
        result: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_picture_get_cull_rect_0_Params {
        p: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_get_cull_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_get_cull_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_picture_get_recording_canvas_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_get_recording_canvas_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_get_unique_id_0_Params {
        p: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_get_unique_id_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_recorder_begin_recording_0_Params {
        r: number;
        rect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_recorder_begin_recording_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_recorder_begin_recording_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_picture_recorder_delete_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_recorder_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_recorder_end_recording_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_recorder_end_recording_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_recorder_end_recording_as_drawable_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_recorder_end_recording_as_drawable_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_picture_unref_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_picture_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_destructor_0_Params {
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_destructor_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_encode_image_0_Params {
        dst: number;
        src: number;
        encoder: number;
        quality: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_encode_image_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_erase_color_0_Params {
        cpixmap: number;
        color: SkiaSharp.SKColor;
        subset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_erase_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_erase_color_0_Return {
        subset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_extract_subset_0_Params {
        cpixmap: number;
        result: number;
        subset: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_extract_subset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_extract_subset_0_Return {
        subset: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_info_0_Params {
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_get_info_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_info_0_Return {
        cinfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_pixel_color_0_Params {
        t: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_get_pixel_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_pixels_0_Params {
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_get_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_pixels_with_xy_0_Params {
        cpixmap: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_get_pixels_with_xy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_get_row_bytes_0_Params {
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_get_row_bytes_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_new_with_params_0_Params {
        cinfo: SkiaSharp.SKImageInfoNative;
        addr: number;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_new_with_params_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_new_with_params_0_Return {
        cinfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_read_pixels_0_Params {
        cpixmap: number;
        dstInfo: SkiaSharp.SKImageInfoNative;
        dstPixels: number;
        dstRowBytes: number;
        srcX: number;
        srcY: number;
        behavior: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_read_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_read_pixels_0_Return {
        dstInfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_reset_0_Params {
        cpixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_reset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_reset_with_params_0_Params {
        cpixmap: number;
        cinfo: SkiaSharp.SKImageInfoNative;
        addr: number;
        rowBytes: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_reset_with_params_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_reset_with_params_0_Return {
        cinfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_pixmap_scale_pixels_0_Params {
        cpixmap: number;
        dst: number;
        quality: number;
        static unmarshal(pData: number, memoryContext?: any): sk_pixmap_scale_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_pngencoder_encode_0_Params {
        dst: number;
        src: number;
        options: SkiaSharp.SKPngEncoderOptions;
        static unmarshal(pData: number, memoryContext?: any): sk_pngencoder_encode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_contains2_0_Params {
        r: number;
        x: number;
        y: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_contains2_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_contains_0_Params {
        r: number;
        region: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_contains_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_get_bounds_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_get_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_get_bounds_0_Return {
        rect: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_region_intersects_0_Params {
        r: number;
        src: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_intersects_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_intersects_rect_0_Params {
        r: number;
        rect: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_region_intersects_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_intersects_rect_0_Return {
        rect: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_region_new2_0_Params {
        r: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_new2_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_op2_0_Params {
        r: number;
        src: number;
        op: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_op2_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_op_0_Params {
        r: number;
        left: number;
        top: number;
        right: number;
        bottom: number;
        op: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_op_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_set_path_0_Params {
        r: number;
        t: number;
        clip: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_set_path_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_set_rect_0_Params {
        r: number;
        rect: SkiaSharp.SKRectI;
        static unmarshal(pData: number, memoryContext?: any): sk_region_set_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_region_set_rect_0_Return {
        rect: SkiaSharp.SKRectI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_region_set_region_0_Params {
        r: number;
        src: number;
        static unmarshal(pData: number, memoryContext?: any): sk_region_set_region_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_contains_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_contains_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_contains_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_delete_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_height_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_get_height_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_radii_0_Params {
        rrect: number;
        corner: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_get_radii_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_radii_0_Return {
        radii: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_rect_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_get_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_type_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_get_type_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_get_width_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_get_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_inset_0_Params {
        rrect: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_inset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_is_valid_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_is_valid_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_new_copy_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_new_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_offset_0_Params {
        rrect: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_offset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_outset_0_Params {
        rrect: number;
        dx: number;
        dy: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_outset_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_empty_0_Params {
        rrect: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_empty_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_nine_patch_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        leftRad: number;
        topRad: number;
        rightRad: number;
        bottomRad: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_nine_patch_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_nine_patch_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_oval_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_oval_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_oval_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_rect_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_radii_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        radii_Length: number;
        radii: Array<SkiaSharp.SKPoint>;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_rect_radii_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_radii_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_xy_0_Params {
        rrect: number;
        rect: SkiaSharp.SKRect;
        xRad: number;
        yRad: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_set_rect_xy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_set_rect_xy_0_Return {
        rect: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_transform_0_Params {
        rrect: number;
        matrix: SkiaSharp.SKMatrix;
        dest: number;
        static unmarshal(pData: number, memoryContext?: any): sk_rrect_transform_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_rrect_transform_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_bitmap_0_Params {
        src: number;
        tmx: number;
        tmy: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_bitmap_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_bitmap_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_bitmap_1_Params {
        src: number;
        tmx: number;
        tmy: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_bitmap_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_color_0_Params {
        color: SkiaSharp.SKColor;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_color_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_color_filter_0_Params {
        proxy: number;
        filter: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_color_filter_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_compose_0_Params {
        shaderA: number;
        shaderB: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_compose_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_compose_with_mode_0_Params {
        shaderA: number;
        shaderB: number;
        mode: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_compose_with_mode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_0_Params {
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_linear_gradient_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_1_Params {
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_linear_gradient_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_2_Params {
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_linear_gradient_2_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_2_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_linear_gradient_3_Params {
        points_Length: number;
        points: Array<SkiaSharp.SKPoint>;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_linear_gradient_3_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_local_matrix_0_Params {
        proxy: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_local_matrix_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_local_matrix_0_Return {
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_fractal_noise_0_Params {
        baseFrequencyX: number;
        baseFrequencyY: number;
        numOctaves: number;
        seed: number;
        tileSizeZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_perlin_noise_fractal_noise_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_fractal_noise_1_Params {
        baseFrequencyX: number;
        baseFrequencyY: number;
        numOctaves: number;
        seed: number;
        tileSize: SkiaSharp.SKPointI;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_perlin_noise_fractal_noise_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_fractal_noise_1_Return {
        tileSize: SkiaSharp.SKPointI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_turbulence_0_Params {
        baseFrequencyX: number;
        baseFrequencyY: number;
        numOctaves: number;
        seed: number;
        tileSizeZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_perlin_noise_turbulence_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_turbulence_1_Params {
        baseFrequencyX: number;
        baseFrequencyY: number;
        numOctaves: number;
        seed: number;
        tileSize: SkiaSharp.SKPointI;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_perlin_noise_turbulence_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_perlin_noise_turbulence_1_Return {
        tileSize: SkiaSharp.SKPointI;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_0_Params {
        center: SkiaSharp.SKPoint;
        radius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_radial_gradient_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_0_Return {
        center: SkiaSharp.SKPoint;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_1_Params {
        center: SkiaSharp.SKPoint;
        radius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_radial_gradient_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_1_Return {
        center: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_2_Params {
        center: SkiaSharp.SKPoint;
        radius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_radial_gradient_2_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_2_Return {
        center: SkiaSharp.SKPoint;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_3_Params {
        center: SkiaSharp.SKPoint;
        radius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_radial_gradient_3_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_radial_gradient_3_Return {
        center: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_0_Params {
        center: SkiaSharp.SKPoint;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        startAngle: number;
        endAngle: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_sweep_gradient_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_0_Return {
        center: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_1_Params {
        center: SkiaSharp.SKPoint;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        startAngle: number;
        endAngle: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_sweep_gradient_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_1_Return {
        center: SkiaSharp.SKPoint;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_2_Params {
        center: SkiaSharp.SKPoint;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        startAngle: number;
        endAngle: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_sweep_gradient_2_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_2_Return {
        center: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_3_Params {
        center: SkiaSharp.SKPoint;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        startAngle: number;
        endAngle: number;
        matrixZero: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_sweep_gradient_3_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_sweep_gradient_3_Return {
        center: SkiaSharp.SKPoint;
        matrixZero: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_0_Params {
        start: SkiaSharp.SKPoint;
        startRadius: number;
        end: SkiaSharp.SKPoint;
        endRadius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_two_point_conical_gradient_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_0_Return {
        start: SkiaSharp.SKPoint;
        end: SkiaSharp.SKPoint;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_1_Params {
        start: SkiaSharp.SKPoint;
        startRadius: number;
        end: SkiaSharp.SKPoint;
        endRadius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPos_Length: number;
        colorPos: Array<number>;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_two_point_conical_gradient_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_1_Return {
        start: SkiaSharp.SKPoint;
        end: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_2_Params {
        start: SkiaSharp.SKPoint;
        startRadius: number;
        end: SkiaSharp.SKPoint;
        endRadius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrix: SkiaSharp.SKMatrix;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_two_point_conical_gradient_2_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_2_Return {
        start: SkiaSharp.SKPoint;
        end: SkiaSharp.SKPoint;
        matrix: SkiaSharp.SKMatrix;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_3_Params {
        start: SkiaSharp.SKPoint;
        startRadius: number;
        end: SkiaSharp.SKPoint;
        endRadius: number;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        colorPosZero: number;
        count: number;
        mode: number;
        matrixZero: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_new_two_point_conical_gradient_3_Params;
    }
}
declare namespace SkiaSharp {
    class sk_shader_new_two_point_conical_gradient_3_Return {
        start: SkiaSharp.SKPoint;
        end: SkiaSharp.SKPoint;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_shader_unref_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_shader_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_asset_destroy_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_asset_destroy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_get_length_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_get_length_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_get_memory_base_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_get_memory_base_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_get_position_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_get_position_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_has_length_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_has_length_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_has_position_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_has_position_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_is_at_end_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_is_at_end_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_move_0_Params {
        stream: number;
        offset: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_move_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_peek_0_Params {
        stream: number;
        buffer: number;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_peek_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_0_Params {
        stream: number;
        buffer: number;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_bool_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_bool_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_bool_0_Return {
        buffer: boolean;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s16_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_s16_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s16_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s32_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_s32_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s32_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s8_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_s8_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_s8_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u16_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_u16_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u16_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u32_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_u32_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u32_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u8_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_read_u8_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_read_u8_0_Return {
        buffer: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_stream_rewind_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_rewind_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_seek_0_Params {
        stream: number;
        position: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_seek_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_stream_skip_0_Params {
        stream: number;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_stream_skip_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_string_destructor_0_Params {
        skstring: number;
        static unmarshal(pData: number, memoryContext?: any): sk_string_destructor_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_string_get_c_str_0_Params {
        skstring: number;
        static unmarshal(pData: number, memoryContext?: any): sk_string_get_c_str_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_string_get_size_0_Params {
        skstring: number;
        static unmarshal(pData: number, memoryContext?: any): sk_string_get_size_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_string_new_with_copy_0_Params {
        src_Length: number;
        src: Array<number>;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_string_new_with_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_draw_0_Params {
        surface: number;
        canvas: number;
        x: number;
        y: number;
        paint: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_draw_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_get_canvas_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_get_canvas_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_get_props_0_Params {
        surface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_get_props_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_backend_render_target_0_Params {
        context: number;
        target: number;
        origin: number;
        colorType: number;
        colorspace: number;
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_backend_render_target_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_backend_texture_0_Params {
        context: number;
        texture: number;
        origin: number;
        samples: number;
        colorType: number;
        colorspace: number;
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_backend_texture_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_backend_texture_as_render_target_0_Params {
        context: number;
        texture: number;
        origin: number;
        samples: number;
        colorType: number;
        colorspace: number;
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_backend_texture_as_render_target_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_image_snapshot_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_image_snapshot_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_null_0_Params {
        width: number;
        height: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_null_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_raster_0_Params {
        info: SkiaSharp.SKImageInfoNative;
        rowBytes: number;
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_raster_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_raster_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_raster_direct_0_Params {
        info: SkiaSharp.SKImageInfoNative;
        pixels: number;
        rowBytes: number;
        releaseProc: number;
        context: number;
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_raster_direct_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_raster_direct_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_render_target_0_Params {
        context: number;
        budgeted: boolean;
        info: SkiaSharp.SKImageInfoNative;
        sampleCount: number;
        origin: number;
        props: number;
        shouldCreateWithMips: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_new_render_target_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_new_render_target_0_Return {
        info: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_surface_peek_pixels_0_Params {
        surface: number;
        pixmap: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_peek_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_read_pixels_0_Params {
        surface: number;
        dstInfo: SkiaSharp.SKImageInfoNative;
        dstPixels: number;
        dstRowBytes: number;
        srcX: number;
        srcY: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_read_pixels_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surface_read_pixels_0_Return {
        dstInfo: SkiaSharp.SKImageInfoNative;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_surface_unref_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surface_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surfaceprops_delete_0_Params {
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surfaceprops_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surfaceprops_get_flags_0_Params {
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surfaceprops_get_flags_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surfaceprops_get_pixel_geometry_0_Params {
        props: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surfaceprops_get_pixel_geometry_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_surfaceprops_new_0_Params {
        flags: number;
        geometry: number;
        static unmarshal(pData: number, memoryContext?: any): sk_surfaceprops_new_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_svgcanvas_create_0_Params {
        bounds: SkiaSharp.SKRect;
        writer: number;
        static unmarshal(pData: number, memoryContext?: any): sk_svgcanvas_create_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_svgcanvas_create_0_Return {
        bounds: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_swizzle_swap_rb_0_Params {
        dest: number;
        src: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_swizzle_swap_rb_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_0_Params {
        builder: number;
        font: number;
        count: number;
        x: number;
        y: number;
        textByteCount: number;
        lang: number;
        bounds: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_alloc_run_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_0_Return {
        runbuffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_pos_0_Params {
        builder: number;
        font: number;
        count: number;
        textByteCount: number;
        lang: number;
        bounds: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_alloc_run_text_pos_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_pos_0_Return {
        runbuffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_pos_h_0_Params {
        builder: number;
        font: number;
        count: number;
        y: number;
        textByteCount: number;
        lang: number;
        bounds: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_alloc_run_text_pos_h_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_alloc_run_text_pos_h_0_Return {
        runbuffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_delete_0_Params {
        builder: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_make_0_Params {
        builder: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_make_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_clusters_0_Params {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        clusters: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_runbuffer_set_clusters_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_clusters_0_Return {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_glyphs_0_Params {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        glyphs: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_runbuffer_set_glyphs_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_glyphs_0_Return {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_pos_0_Params {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        pos: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_runbuffer_set_pos_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_pos_0_Return {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_pos_points_0_Params {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        pos: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_runbuffer_set_pos_points_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_pos_points_0_Return {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_utf8_text_0_Params {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        text: number;
        count: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_builder_runbuffer_set_utf8_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_builder_runbuffer_set_utf8_text_0_Return {
        buffer: SkiaSharp.SKTextBlobBuilderRunBuffer;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_get_bounds_0_Params {
        blob: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_get_bounds_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_get_bounds_0_Return {
        bounds: SkiaSharp.SKRect;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_get_unique_id_0_Params {
        blob: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_get_unique_id_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_ref_0_Params {
        blob: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_ref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_textblob_unref_0_Params {
        blob: number;
        static unmarshal(pData: number, memoryContext?: any): sk_textblob_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_chars_to_glyphs_0_Params {
        t: number;
        chars: number;
        encoding: number;
        glyphPtr: number;
        glyphCount: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_chars_to_glyphs_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_count_tables_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_count_tables_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_create_from_file_0_Params {
        utf8path_Length: number;
        utf8path: Array<number>;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_create_from_file_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_create_from_name_with_font_style_0_Params {
        familyName: string;
        style: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_create_from_name_with_font_style_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_create_from_stream_0_Params {
        stream: number;
        index: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_create_from_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_family_name_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_family_name_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_font_slant_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_font_slant_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_font_weight_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_font_weight_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_font_width_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_font_width_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_fontstyle_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_fontstyle_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_table_data_0_Params {
        typeface: number;
        tag: number;
        offset: number;
        length: number;
        data_Length: number;
        data: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_table_data_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_table_size_0_Params {
        typeface: number;
        tag: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_table_size_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_table_tags_0_Params {
        typeface: number;
        tags_Length: number;
        tags: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_table_tags_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_get_units_per_em_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_get_units_per_em_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_open_stream_0_Params {
        typeface: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_open_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_open_stream_0_Return {
        ttcIndex: number;
        constructor();
        marshalNew(memoryContext?: any): number;
        marshal(pData: number, memoryContext?: any): void;
    }
}
declare namespace SkiaSharp {
    class sk_typeface_unref_0_Params {
        t: number;
        static unmarshal(pData: number, memoryContext?: any): sk_typeface_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_vertices_make_copy_0_Params {
        vmode: number;
        vertexCount: number;
        positions_Length: number;
        positions: Array<SkiaSharp.SKPoint>;
        texs_Length: number;
        texs: Array<SkiaSharp.SKPoint>;
        colors_Length: number;
        colors: Array<SkiaSharp.SKColor>;
        indexCount: number;
        indices_Length: number;
        indices: Array<number>;
        static unmarshal(pData: number, memoryContext?: any): sk_vertices_make_copy_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_vertices_unref_0_Params {
        cvertices: number;
        static unmarshal(pData: number, memoryContext?: any): sk_vertices_unref_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_webpencoder_encode_0_Params {
        dst: number;
        src: number;
        options: SkiaSharp.SKWebpEncoderOptions;
        static unmarshal(pData: number, memoryContext?: any): sk_webpencoder_encode_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_bytes_written_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_bytes_written_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_flush_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_flush_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_get_size_of_packed_uint_0_Params {
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_get_size_of_packed_uint_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_newline_0_Params {
        cstream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_newline_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_0_Params {
        cstream: number;
        buffer: number;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_16_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_16_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_1_Params {
        cstream: number;
        buffer_Length: number;
        buffer: Array<number>;
        size: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_1_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_32_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_32_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_8_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_8_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_bigdec_as_text_0_Params {
        cstream: number;
        value: number;
        minDigits: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_bigdec_as_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_bool_0_Params {
        cstream: number;
        value: boolean;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_bool_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_dec_as_text_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_dec_as_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_hex_as_text_0_Params {
        cstream: number;
        value: number;
        minDigits: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_hex_as_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_packed_uint_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_packed_uint_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_scalar_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_scalar_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_scalar_as_text_0_Params {
        cstream: number;
        value: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_scalar_as_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_stream_0_Params {
        cstream: number;
        input: number;
        length: number;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_stream_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_wstream_write_text_0_Params {
        cstream: number;
        value: string;
        static unmarshal(pData: number, memoryContext?: any): sk_wstream_write_text_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_xmlstreamwriter_delete_0_Params {
        writer: number;
        static unmarshal(pData: number, memoryContext?: any): sk_xmlstreamwriter_delete_0_Params;
    }
}
declare namespace SkiaSharp {
    class sk_xmlstreamwriter_new_0_Params {
        stream: number;
        static unmarshal(pData: number, memoryContext?: any): sk_xmlstreamwriter_new_0_Params;
    }
}
