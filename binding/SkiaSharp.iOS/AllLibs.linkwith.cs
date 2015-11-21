using ObjCRuntime;

[assembly: LinkWith ("libskia_core.a", LinkTarget = LinkTargetHelper.LinkTargets,
	Frameworks = "CoreFoundation CoreGraphics CoreText UIKit Foundation QuartzCore OpenGLES ImageIO Security MobileCoreServices",
	IsCxx = true,
	SmartLink = true, 
	ForceLoad = true)]

[assembly: LinkWith ("libskia_images.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libjpeg-turbo.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libpng_static.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_codec_android.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_codec.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_effects.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_opts.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_ports.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_sfnt.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_skgpu.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libskia_utils.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_dec.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_demux.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_dsp_neon.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_dsp.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_enc.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libwebp_utils.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]


[assembly: LinkWith ("libwebp_dsp_enc.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libSkKTX.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]
[assembly: LinkWith ("libetc1.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]


//[assembly: LinkWith ("libgiflib.a", LinkTarget = LinkTargetHelper.LinkTargets, SmartLink = true, ForceLoad = true)]


struct LinkTargetHelper
{
	public const LinkTarget LinkTargets = LinkTarget.Simulator | LinkTarget.ArmV7;
}
