/*************************************************************************
* ADOBE CONFIDENTIAL
* ___________________
*
*  Copyright 2015 Adobe Systems Incorporated
*  All Rights Reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Adobe Systems Incorporated and its suppliers,
* if any.  The intellectual and technical concepts contained
* herein are proprietary to Adobe Systems Incorporated and its
* suppliers and are protected by all applicable intellectual property laws,
* including trade secret and or copyright laws.
* Dissemination of this information or reproduction of this material
* is strictly forbidden unless prior written permission is obtained
* from Adobe Systems Incorporated.
**************************************************************************/
!function(){let e,t,n,r,a,i,o,s,c,l,d,f,u;const m=chrome.extension.getURL("viewer.html"),g=["https:","http:","file:"],p=e=>{if(!e)return!1;try{const t=new URL(e).protocol;return-1!==g.indexOf(t)}catch(e){return!1}};function h(e){const t=v("search");return new URLSearchParams(t).get(e)}function w(e,t){return new URLSearchParams(e).get(t)||""}function y(e){let t;try{t=localStorage.getItem(e)}catch(e){}return t}function b(e,t){try{localStorage.setItem(e,t)}catch(e){}}function _(e){try{localStorage.removeItem(e)}catch(e){}}function v(e){let t;try{t=sessionStorage.getItem(e)}catch(e){t=""}return t}function E(e,t){try{sessionStorage.setItem(e,t)}catch(e){}}function L(){try{setTimeout(()=>{chrome.tabs.getCurrent((function(e){b("reloadurl-"+e.id,r),window.location.href=r}))},500)}catch(e){T("fallback to native failed",e)}}function R(){setTimeout(()=>{window.location.href=r},200)}!function(){if(r=w(document.location.search,"pdfurl"),!p(r))return void(a=!1);const e=v("search");(!e||w(e,"pdfurl")!==r||e.length<document.location.search)&&E("search",document.location.search),n=w(document.location.search,"pdffilename")||w(e,"pdffilename")||M(r),document.title=n;const t="/"+r+location.hash;history.replaceState({},n,t)}();const S=e=>{try{const t=new URL(y("cdnUrl")),n=[/^https:\/\/([a-zA-Z\d-]+\.){0,}(adobe|acrobat)\.com(:[0-9]*)?$/];return e===t.origin&&!!n.find(t=>t.test(e))}catch(e){return!1}};function I(e){const t={main_op:"analytics"};t.analytics=[[e]],chrome.runtime.sendMessage(t)}const D=["AdobeID","openid","DCAPI","sign_user_read","sign_user_write","sign_user_login","sign_library_read","sign_library_write","agreement_send","agreement_read","agreement_write","ab.manage","additional_info.account_type","sao.ACOM_ESIGN_TRIAL","widget_read","widget_write","workflow_read","workflow_write"],C=y("viewerImsClientId"),P=y("imsURL");function k(){const e=new URL(P+"/ims/authorize/v1?");let t=v("search");t||(t="?pdfurl="+window.location.href.substr(chrome.runtime.getURL("/").length));const n=new URL(m+t);n.hash=n.hash+"signIn=true",e.searchParams.append("response_type","token"),e.searchParams.append("client_id",C),e.searchParams.append("redirect_uri",n),e.searchParams.append("scope",D.join(",")),e.searchParams.append("locale",y("locale")),chrome.tabs.update({url:e.href,active:!0})}const x={isSharePointURL:!1,isSharePointFeatureEnabled:!1,isFrictionlessEnabled:!0,featureFlags:[]};class U{constructor(e){this.iframeElement=void 0,this.parentDiv=e}createIframe=e=>{const t=window.document,n=t.createElement("iframe");n.setAttribute("src",e),n.setAttribute("id","dc-view-frame"),n.setAttribute("allowfullscreen","allowfullscreen"),n.style.width="100vw",n.style.height="100vh",n.style.border="none",n.style.overflow="hidden",this.parentDiv.appendChild(n),this.iframeElement=t.getElementById("dc-view-frame")};_sendMessage=(e,t)=>{this.iframeElement&&S(t)&&function(e){var t=Date.now();return new Promise((function n(r,o){i&&a?r(a):e&&Date.now()-t>=e?o(new Error("timeout")):setTimeout(n.bind(this,r,o),30)}))}(1e6).then(n=>n&&this.iframeElement.contentWindow.postMessage(e,t))};sendFileMetaData=(e,t,n,r,a,i,o)=>{this._sendMessage({fileUrl:a,fileName:i,fileSize:n,acceptRanges:r,handShakeTime:t,type:e,isFrictionlessEnabled:x.isFrictionlessEnabled},o)};sendRecentUrl=(e,t,n,r=!1)=>{this._sendMessage({type:"RecentUrls",permission:e,showOverlay:r,recentUrls:t},n)};sendProgress=(e,t,n,r)=>{this._sendMessage({total:t,loaded:n,type:e},r)};sendInitialBuffer=(e,t,n,r,a)=>{this._sendMessage({type:e,downLoadstartTime:t,downLoadEndTime:n,buffer:r},a)};sendBufferRanges=(e,t,n,r)=>{this._sendMessage({type:e,range:t,buffer:n},r)};preview=(e,t,n,r,a,i,o)=>{this._sendMessage({fileSize:n,type:e,fileBuffer:t,fileName:r,downLoadstartTime:a,downLoadEndTime:i},o)};openInAcrobatResponse=(e,t,n)=>{this._sendMessage({type:e,res:t},n)};postLog=(e,t,n,r,a)=>{this._sendMessage({type:e,reqId:t,message:n,error:r},a)}}function T(e,n){try{d=void 0!==d?d:"false"!==y("logAnalytics"),d&&(c&&t?c.postLog("log",l,e,n,t.origin):setTimeout(()=>{c&&t&&c.postLog("log",l,e,n,t.origin)},500))}catch(e){}}function B(){try{let e=window.location.href;if(e&&e.indexOf("#")>-1){if(e.indexOf("signIn=true")>-1){I("DCBrowserExt:Viewer:Ims:Sign:In:"+(v("signInSource")?v("signInSource"):"Unknown")+":Successful"),I("DCBrowserExt:Viewer:Ims:Sign:In:Successful"),function(e){try{sessionStorage.removeItem(e)}catch(e){}}("signInSource")}e=e.split("#")[0],T("URL hash detected"),window.location.href=e}}catch(e){}}const F=()=>{try{const n=window.document.getElementById("Adobe-dc-view");e=h("clen")||-1,c=new U(n);const r=(()=>{try{const e=y("cdnUrl"),n=new URL(e);S(n.origin)||(T("Invalid CDN URL detected","Invalid Origin"),L()),t||(t=n);let r=y("viewer-locale");r||(r=y("locale"));const a="false"!==y("logAnalytics");n.searchParams.append("locale",r),n.searchParams.append("logAnalytics",a),n.searchParams.append("isDeskTop",y("isDeskTop")),n.searchParams.append("isAcrobat",y("isAcrobat")),n.searchParams.append("callingApp",chrome.runtime.id),"false"===y("staticFteCoachmarkShown")&&n.searchParams.append("showFTECoachmark","true"),"true"===h("googlePrint")&&"false"!==v("googleAppsPrint")&&n.searchParams.append("googleAppsPrint","true");const i=["dropin!","provider!","app!"],o=["analytics","logToConsole","enableLogging","frictionless","sessionId","linearization"],s=(y("env"),new URLSearchParams(window.location.search));s.forEach((e,t)=>{o.forEach(r=>{t===r&&n.searchParams.append(t,e)})});let c=n.href;return s.forEach((e,t)=>{i.forEach(n=>{t.startsWith(n)&&(c=c+"&"+t+"="+e)})}),c}catch(e){T("Iframe src creation failed",e),L()}})();c.createIframe(r),window.addEventListener("message",e=>{!e.data||!S(e.origin)||o||"hsready"!==e.data.type&&"ready"!==e.data.type||(o=!0,s=(new Date).getTime(),l=e.data.requestId,"on"===e.data.killSwitch?(I("DCBrowserExt:Viewer:KillSwitch:Turned:On"),T("KillSwitch turned On","KillSwitch"),b("pdfViewer",!1),b("killSwitch","on"),R()):y("killSwitch")&&(I("DCBrowserExt:Viewer:KillSwitch:Turned:Off"),T("KillSwitch turned Off"),_("killSwitch")),T("Handshake done"))})}catch(e){T("Error create Iframe",e)}};function M(e){if(n)return n;let t=e;try{const n=e.split("?")[0].split("/").filter(e=>e.length>0),r=n.length>0?n[n.length-1]:"untitled";t=r;const a=r.length-4;(r.length<4||r.toLowerCase().indexOf(".pdf")!==a)&&(t+=".pdf")}catch(e){T("Error in getFileNameFromURL",e)}return t}function V(t,n){return new Promise((r,a)=>{const i=(new Date).getTime(),o=new XMLHttpRequest;o.open("GET",t.url),o.responseType="arraybuffer",o.setRequestHeader("Range",`bytes=${n.start}-${n.end}`),o.onload=()=>{if(4===o.readyState&&206===o.status)r({buffer:o.response,startTime:i,endTime:(new Date).getTime()});else if(200===o.status){const t={status:o.status,statusText:o.statusText,fileSize:e,rangeBufferSize:o.response.byteLength,range:n};a({message:"Unexpected response to get file buffer range",error:t})}else{const t={status:o.status,statusText:o.statusText,fileSize:e,range:n};a({message:"Invalid response to get file buffer ranger",error:t})}},o.onerror=e=>{a({message:"Error to get file buffer range",error:e})},o.ontimeout=e=>{a({message:"Timeout to get file buffer range due to timeout",error:e})},o.send()})}function z(e,t){"PDF"===function(e){if(e)try{var t=new URL(e).pathname;return t.substr(t.lastIndexOf(".")+1).toUpperCase()}catch(e){return""}return""}(e)&&(a=!0);const n=new XMLHttpRequest;n.open("GET",e),n.responseType="arraybuffer",n.onreadystatechange=function(){4===n.readyState&&(200!==n.status&&0!=n.status||t({buffer:n.response,mimeType:n.getResponseHeader("content-type")}))},n.send(null)}function A(t,n,i){return new Promise((o,s)=>{const c=r;if(c.startsWith("file://"))return void z(c,o);const l=new XMLHttpRequest;var d;l.open("GET",c),l.responseType="arraybuffer",n&&l.setRequestHeader("If-Range","randomrange"),l.onreadystatechange=(d=l,function(e){if(this.readyState==this.HEADERS_RECEIVED){if(!function(e,t){const n=e.getResponseHeader("content-type"),r=e.getResponseHeader("content-disposition");if(n){const e=n.toLowerCase().split(";",1)[0].trim();if(r&&/^\s*attachment[;]?/i.test(r.value))return!1;if("application/pdf"===e)return!0;if("application/octet-stream"===e&&r&&/\.pdf(["']|$)/i.test(r.value))return!0}return!1}(d))return T("Fall back to native - not pdf from headers"),R();a=!0}}),l.onprogress=function(t,n){return function(r){r.lengthComputable&&(e=r.total,t.sendProgress("progress",r.total,r.loaded,n))}}(t,i),l.onload=()=>{if(l.status>=200&&l.status<400)o({buffer:l.response,mimeType:l.getResponseHeader("content-type"),downLoadEndTime:(new Date).getTime()});else{const e={status:l.status,statusText:l.statusText};s({message:"Invalid response fetching content",error:e})}},l.onerror=e=>{s({message:"Error to download file contents",error:e})},l.ontimeout=e=>{s({message:"Timeout to download file contents",error:e})},l.send()})}function O(e,t,n=!1){chrome.history.search({text:chrome.extension.getURL("viewer.html"),startTime:0,maxResults:1e3},(function(r){const a=r.filter(e=>e.url.startsWith(chrome.extension.getURL("viewer.html"))),i=[];for(let e=0;e<a.length;++e){const{url:t,title:n}=a[e],{lastVisitTime:r}=a[e];i.push({filename:n,url:t,lastVisited:r,chromeHistory:!0})}e.sendRecentUrl(!0,i,t,n)}))}function H(e,t){switch(t.data.main_op){case"open_in_acrobat":case"fillsign":!function(e,t){const n={main_op:"open_in_acrobat"};if("fillsign"===t.data.main_op&&(n.paramName="FillnSign"),n.url=t.data.url,n.click_context="pdfviewer",n.timeStamp=Date.now(),t.data.fileBuffer){const e=new Blob([t.data.fileBuffer],{type:"application/pdf"});n.dataURL=URL.createObjectURL(e)}function r(n){"fillsign"===t.data.main_op?e.openInAcrobatResponse("FILLSIGN_IN_DESKTOP_APP",n,t.origin):e.openInAcrobatResponse("OPEN_IN_DESKTOP_APP",n,t.origin),T(`Open In Acrobat - (${t.data.main_op}) response- ${n}`)}"true"===y("isSharepointFeatureEnabled")?x.isSharePointURL?(n.workflow_name="SharePoint",n.isSharePointURL=!0,chrome.runtime.sendMessage(n,r)):util.checkForSharePointURL(n.url,e=>{n.isSharePointURL=e,n.isSharePointURL&&(n.workflow_name="SharePoint"),chrome.runtime.sendMessage(n,r)}):chrome.runtime.sendMessage(n,r)}(e,t);break;case"complete_conversion":I("DCBrowserExt:Viewer:Verbs:Conversion:Redirection"),function(e){const t={};t.main_op=e.data.main_op,t.conversion_url=decodeURIComponent(e.data.conversion_url),t.timeStamp=Date.now(),chrome.runtime.sendMessage(t)}(t);break;case"updateLocale":I("DCBrowserExt:Viewer:User:Locale:Updated"),b("viewer-locale",t.data.locale),chrome.tabs.reload();break;case"setInitialLocale":let n=!1;y("viewer-locale")||(n=!0,b("viewer-locale",t.data.locale),I("DCBrowserExt:Viewer:User:Locale:Initial")),t.data.reloadReq&&n&&chrome.tabs.reload();break;case"deleteViewerLocale":y("viewer-locale")&&(_("viewer-locale"),chrome.tabs.reload());break;case"signin":I("DCBrowserExt:Viewer:Ims:Sign:In"),E("signInSource",t.data.source),k();break;case"fetchLocalRecents":const r=new URL(y("cdnUrl")).origin;chrome.permissions.contains({permissions:["history"],origins:["https://www.google.com/"]},e=>{if(t.data.fetchRecents){const n=t.data.showOverlay;e?O(c,r,n):(I("DCBrowserExt:Permissions:History:DialogTriggered"),chrome.permissions.request({permissions:["history"],origins:["https://www.google.com/"]},e=>{e?(I("DCBrowserExt:Permissions:History:Granted"),O(c,r,n)):(I("DCBrowserExt:Permissions:History:Denied"),c.sendRecentUrl(!1,null,r))}))}else e?c.sendRecentUrl(!0,null,r):c.sendRecentUrl(!1,null,r)})}}function N(a,o,l,d,f){i=!0;const u=r,m=h("chunk")||"false";a.sendFileMetaData("metadata",s,e,m,encodeURI(u),n,o.origin),l?(I("DCBrowserExt:Viewer:Linearization:Range:Supported"),T("Range call supported"),l.then(e=>{a.sendInitialBuffer("initialBuffer",e.startTime,e.endTime,e.buffer,o.origin),function(e){try{const n=new TextDecoder("utf-8").decode(e.buffer);let r=!1;-1!=n.indexOf("Linearized 1")?(r=!0,I("DCBrowserExt:Viewer:Linearization:Linearized:Version:1"),T("Linearized PDF v1 detected")):-1!=n.indexOf("Linearized")?(I("DCBrowserExt:Viewer:Linearization:Linearized:Version:Other"),T("Linearized PDF other version detected")):(I("DCBrowserExt:Viewer:Linearization:Linearized:False"),T("Non Linearized PDF detected")),c._sendMessage({type:"Linearization",linearized:r},t.origin)}catch(e){I("DCBrowserExt:Viewer:Linearization:Linearized:Detection:Failed"),T("Linearization Detection failed",e)}}(e)}).catch(e=>{a.sendInitialBuffer("initialBuffer",0,0,-1,o.origin),I("DCBrowserExt:Viewer:Error:Linearization:InitialBuffer:Failed"),T("Initial buffer download failed",e)})):(I("DCBrowserExt:Viewer:Linearization:Range:Not:Supported"),T("Range call not supported"),a.sendInitialBuffer("initialBuffer",0,0,-1,o.origin)),d.then(r=>{const i=r.downLoadEndTime,s=r.buffer;r.buffer.byteLength;a.preview("preview",s,e,n,f,i,o.origin),c._sendMessage({type:"NavigationStartTime",time:window.performance&&window.performance.timing&&window.performance.timing.navigationStart},t.origin)}).catch(e=>(I("DCBrowserExt:Viewer:Error:FallbackToNative:FileDownload:Failed"),T("File download failed, falling back to native viewer",e),L())),T("Viewer loaded")}function $(e,t,n,a){return i=>{try{if(i.data&&i.origin&&S(i.origin)&&(e=>{try{return e&&e.source&&e.source.top.location.origin==="chrome-extension://"+chrome.runtime.id}catch(e){return!1}})(i)){if(i.data.main_op)return H(e,i);switch(i.data.type){case"ready":N(e,i,n,t,a);break;case"getFileBufferRange":!function(e,t){V({url:r},e.data.range).then(n=>{u||(I("DCBrowserExt:Viewer:Linearization:Range:Called"),T("Range call received"),u=!0),t.sendBufferRanges("bufferRanges",`${e.data.range.start}-${e.data.range.end}`,n.buffer,e.origin)}).catch(n=>{I("DCBrowserExt:Viewer:Error:Linearization:Range:Failed"),T(`Range buffer download failed for ${e.data.range.start}-${e.data.range.end}`,n),t.sendBufferRanges("bufferRanges",`${e.data.range.start}-${e.data.range.end}`,-1,e.origin)})}(i,e);break;case"previewFailed":f||(I("DCBrowserExt:Viewer:Error:FallbackToNative:Preview:Failed"),T("File preview failed, falling back to native viewer","previewFailed"),f=!0,L());break;case"signin":I("DCBrowserExt:Viewer:Ims:Sign:In"),k();break;case"signout":I("DCBrowserExt:Viewer:Ims:Sign:Out"),_("viewer-locale"),function(){const e=new URL(P+"/ims/logout/v1?");e.searchParams.append("client_id",C),e.searchParams.append("redirect_uri",window.location.href),chrome.tabs.update({url:e.href,active:!0})}();break;case"coachMarkClosed":b("staticFteCoachmarkShown","true"),I("DCBrowserExt:Staticpdf:fte:CoachMark:Closed");break;case"coachmarkManageSettings":chrome.runtime.openOptionsPage(),b("staticFteCoachmarkShown","true"),I("DCBrowserExt:Staticpdf:fte:CoachMark:ManageSettings:clicked");break;case"coachMarkDisplayed":b("staticFteCoachmarkShown","true"),I("DCBrowserExt:Staticpdf:fte:CoachMark:Shown");break;case"googleAppsPrintShown":E("googleAppsPrint","false"),I("DCBrowserExt:Viewer:GoogleApps:Print:Shown")}}}catch(e){I("DCBrowserExt:Viewer:Error:MessageHandler:Unknown"),T("Unknown error","MessageHandler")}}}function q(){if(!o)return I("DCBrowserExt:Viewer:Error:Handshake:TimedOut"),T("Handshake timed out - falling back to native","Timeout"),L(),!1}function W(e){const t=document.getElementById("__acrobatDialog__");t&&0!==t.length?t&&"none"===t.style.display&&"visible"===e.frame_visibility?t.style.display="block":t&&e.trefoilClick&&(delete e.trefoilClick,t.remove()):function(e){const t=e.base64PDF;delete e.base64PDF;const n="message="+encodeURIComponent(JSON.stringify(e));e.base64PDF=t;const r=null===e.locale;let a=e.version>13||r?"210px":"130px",i="block";"hidden"===e.frame_visibility&&(i="none");const o=document.createElement("iframe");o.setAttribute("src",`${chrome.extension.getURL("data/js/frame.html")}?${n}`),o.setAttribute("id","__acrobatDialog__"),o.style.border="0px",o.style.zIndex="999999999999",o.style.position="fixed",o.style.top="-5px",o.style.right="80px",o.style.width="294px",o.style.height=a,o.style.display=i,o.style.margin="auto",document.getElementById("trefoil_m").appendChild(o)}(e)}(()=>{try{if(B(),!p(r))return void(a=!1);F();const e=h("clen")||-1,i=h("chunk")||!1,o="false"!==h("linearization"),s={url:r},l=(new Date).getTime(),d=new URL(y("cdnUrl"));n=h("pdffilename")||M(r),document.title=decodeURIComponent(n),t||(t=d);let f=null;const u=o&&i&&e>0;u&&(f=V(s,{start:0,end:1024}));const m=A(c,u,d.origin);window.addEventListener("message",$(c,m,f,l)),setTimeout(q,25e3)}catch(e){T("InitScript failed",e),L()}})(),document.addEventListener("DOMContentLoaded",function(e){const t=(new Date).getTime();var n=window.setInterval((function(){(function(){const e=document.getElementById("dc-view-frame");return e&&e.contentWindow&&1===e.contentWindow.length}()||(new Date).getTime()-t>15e3)&&(window.clearInterval(n),e.call(this))}),200)}((function(){const e=document.getElementById("dc-view-frame");e&&e.contentWindow&&e.contentWindow.focus()}))),void 0!==chrome.runtime&&(chrome.runtime.onMessage.addListener((function(e){if(e.panel_op&&(1==e.trefoilClick?(delete e.trefoilUI,delete e.newUI,W(e)):!0===e.reload_in_native&&(delete e.is_viewer,chrome.tabs.reload(e.tabId))),"relay_to_content"!==e.main_op||"dismiss"!==e.content_op)return"viewer-startup-response"===e.main_op?(x.isSharePointURL=!!e.isSharePointURL,x.isSharePointFeatureEnabled=!!e.isSharePointEnabled,x.isFrictionlessEnabled=!!e.isFrictionlessEnabled,x.featureFlags=!!e.featureFlags):"reset"===e.main_op&&c._sendMessage({type:"toggleAnalytics",logAnalytics:e.analytics_on},t.origin),!1;{delete e.content_op,delete e.trefoilClick,delete e.reload_in_native;let t=document.getElementById("__acrobatDialog__");t&&(t.remove(),t=null)}})),chrome.runtime.sendMessage({main_op:"viewer-startup",url:document.location.href,startup_time:Date.now(),viewer:!0}))}();