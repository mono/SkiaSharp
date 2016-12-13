//
//  SkiaKeeper.c
//
//  Created by Bill Holmes on 11/23/15.
//  Copyright © 2015 Xamarin. All rights reserved.
//

#include "sk_xamarin.h"

// Skia
#include "sk_bitmap.h"
#include "sk_canvas.h"
#include "sk_codec.h"
#include "sk_colorfilter.h"
#include "sk_colortable.h"
#include "sk_data.h"
#include "sk_document.h"
#include "sk_image.h"
#include "sk_imagefilter.h"
#include "sk_maskfilter.h"
#include "sk_mask.h"
#include "sk_matrix.h"
#include "sk_paint.h"
#include "sk_path.h"
#include "sk_patheffect.h"
#include "sk_picture.h"
#include "sk_region.h"
#include "sk_shader.h"
#include "sk_shader.h"
#include "sk_stream.h"
#include "sk_string.h"
#include "sk_surface.h"
#include "sk_typeface.h"
#include "gr_context.h"

// Xamarin
#include "sk_managedstream.h"

SK_X_API void** KeepSkiaCSymbols ();

void** KeepSkiaCSymbols ()
{
    static void* ret[] = {
        // Skia
        (void*)sk_canvas_save,
        (void*)sk_data_new_empty,
        (void*)sk_image_new_raster_copy,
        (void*)sk_maskfilter_ref,
        (void*)sk_paint_new,
        (void*)sk_path_new,
        (void*)sk_picture_recorder_new,
        (void*)sk_region_new,
        (void*)sk_shader_ref,
        (void*)sk_surface_new_raster,
        (void*)sk_colortype_get_default_8888,
        (void*)sk_bitmap_new,
        (void*)sk_canvas_clear,
        (void*)sk_mask_new,
        (void*)sk_colorfilter_unref,
        (void*)sk_data_new_from_file,
        (void*)sk_image_encode_specific,
        (void*)sk_image_new_from_bitmap,
        (void*)sk_codec_new_from_stream,
        (void*)sk_imagefilter_croprect_new,
        (void*)sk_maskfilter_new_emboss,
        (void*)sk_paint_is_dither,
        (void*)sk_path_rmove_to,
        (void*)sk_shader_new_empty,
        (void*)sk_filestream_new,
        (void*)sk_memorystream_new,
        (void*)sk_stream_read,
        (void*)sk_typeface_create_from_name,
        (void*)sk_string_new_empty,
        (void*)sk_document_unref,
        (void*)sk_wstream_write,
        (void*)sk_path_effect_create_dash,
        (void*)sk_matrix_concat,
        (void*)sk_matrix_pre_concat,
        (void*)sk_matrix_post_concat,
        (void*)sk_matrix_map_rect,
        (void*)sk_matrix_map_points,
        (void*)sk_matrix_map_vectors,
        (void*)sk_matrix_map_xy,
        (void*)sk_matrix_map_vector,
        (void*)sk_matrix_map_radius,
        (void*)sk_matrix_try_invert,
        (void*)sk_colortable_new,
        (void*)gr_context_unref,

        // Xamarin
        (void*)sk_managedstream_new,
        (void*)sk_force_linking,

        // Test Cases
        (void*)sk_surface_unref,
        (void*)sk_typeface_unref,
        (void*)sk_canvas_draw_color,
        (void*)sk_paint_set_textsize,
    };
    return ret;
}
