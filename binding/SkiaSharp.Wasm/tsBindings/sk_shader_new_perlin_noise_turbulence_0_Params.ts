/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_perlin_noise_turbulence_0_Params
	{
		/* Pack=4 */
		baseFrequencyX : number;
		baseFrequencyY : number;
		numOctaves : number;
		seed : number;
		tileSizeZero : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_perlin_noise_turbulence_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_perlin_noise_turbulence_0_Params();
			
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
				ret.tileSizeZero = Number(memoryContext.getValue(pData + 16, "*"));
			}
			return ret;
		}
	}
}
