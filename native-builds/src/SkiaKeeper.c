//
//  SkiaKeeper.c
//
//  Created by Bill Holmes on 11/23/15.
//  Copyright © 2015 Xamarin. All rights reserved.
//

#include "sk_xamarin.h"

// Google Skia
#include "sk_canvas.h"
#include "sk_data.h"
#include "sk_image.h"
#include "sk_maskfilter.h"
#include "sk_matrix.h"
#include "sk_paint.h"
#include "sk_path.h"
#include "sk_picture.h"
#include "sk_shader.h"
#include "sk_surface.h"

// Xamarin Skia
#include "xamarin/sk_x_bitmap.h"
#include "xamarin/sk_x_canvas.h"
#include "xamarin/sk_x_colorfilter.h"
#include "xamarin/sk_x_data.h"
#include "xamarin/sk_x_image.h"
#include "xamarin/sk_x_imagedecoder.h"
#include "xamarin/sk_x_imagefilter.h"
#include "xamarin/sk_x_maskfilter.h"
#include "xamarin/sk_x_paint.h"
#include "xamarin/sk_x_path.h"
#include "xamarin/sk_x_shader.h"
#include "xamarin/sk_x_stream.h"
#include "xamarin/sk_x_typeface.h"
#include "xamarin/sk_x_string.h"

// Xamarin
#include "sk_managedstream.h"

SK_X_API void** KeepSkiaCSymbols ();

void** KeepSkiaCSymbols ()
{
    static void* ret[] = {
        // Google Skia
        (void*)sk_canvas_save,
        (void*)sk_data_new_empty,
        (void*)sk_image_new_raster_copy,
        (void*)sk_maskfilter_ref,
        (void*)sk_paint_new,
        (void*)sk_path_new,
        (void*)sk_picture_recorder_new,
        (void*)sk_shader_ref,
        (void*)sk_surface_new_raster,
        (void*)sk_colortype_get_default_8888,

        // Xamarin Skia
        (void*)sk_bitmap_new,
        (void*)sk_canvas_clear,
        (void*)sk_colorfilter_unref,
        (void*)sk_data_new_from_file,
        (void*)sk_image_encode_specific,
        (void*)sk_imagedecoder_destructor,
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
