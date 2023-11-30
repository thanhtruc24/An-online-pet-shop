namespace CHTC_1.Avatar
{
    public class GravatarService : IGravatarService
    {
        private readonly IGravatarService _gravatarService;

        public GravatarService(IGravatarService gravatarService)
        {
            _gravatarService = gravatarService;
        }

        public string GetAvatarURL(string email)
        {
            // Sử dụng GravatarService để lấy URL hình ảnh Gravatar
            return _gravatarService.GetAvatarURL(email);
        }
    }
}
