(function(owner) {
   
    owner.sdk = new WSDK();
    owner.appkey = null;
	window.__WSDK__POSTMESSAGE__DEBUG__ = function(error){
        console.log("__WSDK__POSTMESSAGE__DEBUG__" + error); 
    };




    /**
	 * chat IM 登陸
	 * @param {Object} userId
	 * @param {Object} callback
	 */
	owner.chatLogin = function (userId,apiKey, callback) {
	    owner.appkey = apiKey;
	    owner.sdk.Base.login({
	        uid: userId,
	        appkey: apiKey,
	        credential: userId,
	        timeout: 5000,
	        success: function (data) {
	            owner.sdk.Base.startListenAllMsg();
	            console.log('login success:' + JSON.stringify(data));
	            if (callback) {
	                callback();
	            }
	        },
	        error: function (err) {
	            console.log('login fail:' + JSON.stringify(err));
	        }
	    });
	};

	owner.getUserStatus = function (message, callback) {
	    var eids = message.data;
	    if (!eids || eids.length < 1) {
	        return;
	    }
	    owner.sdk.Chat.getUserStatus({
	        uids: eids,
	        hasPrefix: false,
	        appkey:owner.appkey,
	        success: function (data) {
	            console.log(data);
	            if (callback) {
	                callback(data);
	            }
	        },
	        error: function () {
	            console.log('批量获取用户在线状态失败');
	        }
	    });
	}
}(window.openIm = {}));