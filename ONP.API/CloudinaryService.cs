using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;

namespace ONP.API
{

	public class CloudinaryService
	{
		private readonly Cloudinary _cloudinary;

		public CloudinaryService(IConfiguration config)
		{
			var acc = new Account(
				config["CloudinarySettings:CloudName"],
				config["CloudinarySettings:ApiKey"],
				config["CloudinarySettings:ApiSecret"]
			);

			_cloudinary = new Cloudinary(acc);
		}

		public async Task<string> UploadImageAsync(IFormFile file)
		{
			using var stream = file.OpenReadStream();

			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(file.FileName, stream),
				Folder = "profile-images" // ممكن تغيره
			};

			var result = await _cloudinary.UploadAsync(uploadParams);
			return result.SecureUrl.ToString();
		}
	}
}
