namespace SkiaSharp
{
    /// <summary>
    /// For container formats that contain both still images and image sequences,
    /// <br></br> instruct the decoder how the output should be selected. (Refer to comments
    /// <br></br> for each value for more details.)
    /// </summary>
    public enum SKCodecSelectionPolicy
	{
		/// <summary>
		/// If the container format contains both still images and image sequences,
		/// <br></br> SKCodec should choose one of the still images. This is the default.
		/// </summary>
		preferStillImage,
		/// <summary>
		/// If the container format contains both still images and image sequences,
		/// <br></br> SKCodec should choose one of the image sequences for animation.
		/// </summary>
		preferAnimation
	}
}
