/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_imagefilter_new_matrix_convolution_0_Params
	{
		/* Pack=4 */
		kernelSize : SkiaSharp.SKSizeI;
		kernel_Length : number;
		kernel : Array<number>;
		gain : number;
		bias : number;
		kernelOffset : SkiaSharp.SKPointI;
		tileMode : number;
		convolveAlpha : boolean;
		input : number;
		cropRect : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_imagefilter_new_matrix_convolution_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_imagefilter_new_matrix_convolution_0_Params();
			
			{
				ret.kernelSize = SkiaSharp.SKSizeI.unmarshal(pData + 0);
			}
			
			{
				ret.kernel_Length = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 12, "*");
				if(pArray !== 0)
				{
					ret.kernel = new Array<number>();
					for(var i=0; i<ret.kernel_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "float");
						ret.kernel.push(Number(value));
					}
				}
				else
				
				{
					ret.kernel = null;
				}
			}
			
			{
				ret.gain = Number(memoryContext.getValue(pData + 16, "float"));
			}
			
			{
				ret.bias = Number(memoryContext.getValue(pData + 20, "float"));
			}
			
			{
				ret.kernelOffset = SkiaSharp.SKPointI.unmarshal(pData + 24);
			}
			
			{
				ret.tileMode = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			
			{
				ret.convolveAlpha = Boolean(memoryContext.getValue(pData + 36, "i32"));
			}
			
			{
				ret.input = Number(memoryContext.getValue(pData + 40, "*"));
			}
			
			{
				ret.cropRect = Number(memoryContext.getValue(pData + 44, "*"));
			}
			return ret;
		}
	}
}
