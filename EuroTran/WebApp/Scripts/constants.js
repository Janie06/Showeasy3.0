var APP_CLIENT_ID = "8509ba61-9451-4960-94d8-10934d5365cc";
var REDIRECT_URL = "https://www.origtek.com:8016/Page/Calendar_Test.html";

var authEndpoint = 'https://login.microsoftonline.com/common/oauth2/v2.0/authorize?';
var redirectUri = 'https://www.origtek.com:8016/Page/Calendar_Test.html';
var appId = APP_CLIENT_ID;
var scopes = 'openid profile User.Read Mail.Read';

function buildAuthUrl() {
    // Generate random values for state and nonce
    sessionStorage.authState = guid();
    sessionStorage.authNonce = guid();

    var authParams = {
        response_type: 'id_token token',
        client_id: appId,
        redirect_uri: redirectUri,
        scope: scopes,
        state: sessionStorage.authState,
        nonce: sessionStorage.authNonce,
        response_mode: 'fragment'
    };

    return authEndpoint + $.param(authParams);
}

// Helper method to validate token and refresh
// if needed
function getAccessToken(callback) {
    var now = new Date().getTime();
    var isExpired = now > parseInt(sessionStorage.tokenExpires);
    // Do we have a token already?
    if (sessionStorage.accessToken && !isExpired) {
        // Just return what we have
        if (callback) {
            callback(sessionStorage.accessToken);
        }
    } else {
        // Attempt to do a hidden iframe request
        makeSilentTokenRequest(callback);
    }
}