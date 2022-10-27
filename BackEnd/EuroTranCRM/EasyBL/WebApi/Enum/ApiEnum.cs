using EasyBL.WebApi.Common;

namespace EasyBL.WebApi
{
    public enum StatusCodeEnum
    {
        [Text("請求(或處理)成功")]
        Success = 200, //請求(或處理)成功

        [Text("內部請求錯誤")]
        Error = 500, //內部請求錯誤

        [Text("未授權標識")]
        Unauthorized = 401,//未授權標識

        [Text("請求參數不完整或不正確")]
        ParameterError = 400,//請求參數不完整或不正確

        [Text("請求TOKEN失效")]
        TokenInvalid = 403,//請求TOKEN失效

        [Text("您的身份認證已經過期，請重新登入")]
        TokenVerifyFailed = 404,//您的身份認證已經過期，請重新登入

        [Text("HTTP請求類型不合法")]
        HttpMehtodError = 405,//HTTP請求類型不合法

        [Text("HTTP請求不合法,請求參數可能被篡改")]
        HttpRequestError = 406,//HTTP請求不合法

        [Text("該URL已經失效")]
        URLExpireError = 407,//該URL已經失效
    }
}