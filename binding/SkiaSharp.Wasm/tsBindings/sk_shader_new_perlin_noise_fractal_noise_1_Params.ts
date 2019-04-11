/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_perlin_noise_fractal_noise_1_Params
	{
		/* Pack=4 */
		baseFrequencyX : number;
		baseFrequencyY : number;
		numOctaves : number;
		seed : number;
		tileSize : SkiaSharp.SKPointI;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_perlin_noise_fractal_noise_1_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_perlin_noise_fractal_noise_1_Params();
			
			{
				ret.baseFrequencyX = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.baseFrequencyY = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.numOctaves = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				ret.seed = Number(memoryContext.getValue(pData + 12, "float"));
			}
			
			{
				ret.tileSize = SkiaSharp.SKPointI.unmarshal(pData + 16);
			}
			return ret;
		}
	}
}
