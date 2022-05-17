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
var communicate,acom_analytics,utilities,mvTracking,started,installSource,startup=new Promise((function(e,t){"use strict";started=e}));function registerActions(e){"use strict";var t,n=function(){return communicate.getModule("acro-web2pdf")},o=function(){return communicate.getModule("acro-gstate")},a=["*://*/*.pdf"],i=["http://*/*","https://*/*"],r=["all"],c=function(e){var t,n=e.splice();for(t=0;t<e.length;t+=1)n.push(e[t]+"?*")}(["*://*/*.ai","*://*/*.bmp","*://*/*.doc","*://*/*.docx","*://*/*.gif","*://*/*.indd","*://*/*.jpeg","*://*/*.jpg","*://*/*.odf","*://*/*.odg","*://*/*.odp","*://*/*.ods","*://*/*.odt","*://*/*.png","*://*/*.ppt","*://*/*.pptx","*://*/*.pptx","*://*/*.ps","*://*/*.psd","*://*/*.pub","*://*/*.rtf","*://*/*.stw","*://*/*.sxd","*://*/*.sxc","*://*/*.sxi","*://*/*.sxw","*://*/*.text","*://*/*.tif","*://*/*.tiff","*://*/*.txt","*://*/*.xls","*://*/*.xlsx"].concat(a));function l(e){return utilities&&utilities.isChromeOnlyMessage(e)&&utilities.isEdge()&&(e+="Edge"),utilities&&utilities.getTranslation?utilities.getTranslation(e):chrome.i18n.getMessage(e)}function s(e){return(e.title||l("web2pdfUntitledFileName")).replace(/[<>?:|\*"\/\\'&\.]/g,"")}function E(e,t){if(!e&&!t)return!1;try{const n=e.pageUrl||t.url,o=new URL(n);if(o.protocol&&["http:","https:"].includes(o.protocol))return!0}catch(e){console.error(e)}return!1}startupComplete||(startupComplete=!0,startup.then((function(t){chrome.runtime.getPlatformInfo((function(e){var t;SETTINGS.OS=e.os,SETTINGS.CHROME_VERSION=0,SETTINGS.EXTENSION_VERSION=0;try{(t=navigator.userAgent.match(/Chrome\/([0-9]+)/))&&(SETTINGS.CHROME_VERSION=+t[1])}catch(e){}try{SETTINGS.EXTENSION_VERSION=chrome.runtime.getManifest().version}catch(e){}"mac"===e.os?acom_analytics.event(acom_analytics.e.OS_MAC_OP):"win"===e.os&&acom_analytics.event(acom_analytics.e.OS_WIN_OP)})),e?"update"===e.reason?t.event(t.e.EXTENSION_UPDATE):"install"===e.reason&&(t.event(t.e.EXTENSION_INSTALLED),chrome.management.getSelf((function(e){switch(e.installType){case"admin":t.event(t.e.EXTENSION_INSTALLED_ADMIN);break;case"development":t.event(t.e.EXTENSION_INSTALLED_DEVELOPMENT);break;case"other":t.event(t.e.EXTENSION_INSTALLED_OTHER);break;case"normal":t.event(t.e.EXTENSION_INSTALLED_DIRECT);let e="store_direct";mvTracking&&(e=mvTracking),t.event(t.e.EXTENSION_INSTALLED_SOURCE,{SOURCE:e});break;case"sideload":t.event(t.e.EXTENSION_INSTALLED_SIDE_LOADED),SETTINGS.IS_READER&&"win"===SETTINGS.OS&&t.event(t.e.EXTENSION_INSTALLED_SIDE_LOADED_SOURCE,{SOURCE:installSource})}}))):t.event(t.e.EXTENSION_STARTUP),chrome.browserAction.onClicked.addListener((function(e){communicate.echo(e)}));try{if(window.navigator.onLine){const e=localStorage.getItem("pdfViewer"),o=localStorage.getItem("killSwitch"),a=localStorage.getItem("cdnUrl");"false"===e&&"on"===o&&(n=a,new Promise((function(e,t){let o=new XMLHttpRequest;o.open("GET",n),o.timeout=5e3,o.onload=function(){if(4===o.readyState)return 200===this.status?e(o.response):t({statusText:o.statusText})},o.onerror=function(){return t({statusText:o.statusText})},o.ontimeout=()=>t({statusText:"Failed due to timeout"}),o.send()}))).then(e=>{-1===e.toString().indexOf("<meta name='killSwitch' content='off'/>")&&-1===e.toString().indexOf('<meta name="killSwitch" content="off"/>')||(localStorage.setItem("pdfViewer",!0),localStorage.setItem("killSwitch","off"),t.event(t.e.VIEWER_KILL_SWITCH_OFF_SUCCESS))}).catch(e=>{t.event(t.e.VIEWER_KILL_SWITCH_OFF_FAILED)})}}catch(e){t.event(t.e.VIEWER_KILL_SWITCH_OFF_FAILED)}var n})),!SETTINGS.IS_READER&&SETTINGS.USE_ACROBAT?(chrome.contextMenus.create({title:l("web2pdfConvertPageContextMenu"),contexts:["page"],onclick:function(e,t){E(e,t)&&(acom_analytics.event(acom_analytics.e.CONTEXT_MENU_CONVERT_PAGE),n().handleConversionRequest({tabId:t.id,caller:o().web2pdfCaller.MENU,action:o().web2pdfAction.CONVERT,context:o().web2pdfContext.PAGE,url:e.pageUrl||t.url,domtitle:s(t)}))},documentUrlPatterns:i,id:"convertPageContextMenu"}),chrome.contextMenus.create({title:l("web2pdfAppendPageContextMenu"),contexts:["page"],onclick:function(e,t){E(e,t)&&(acom_analytics.event(acom_analytics.e.CONTEXT_MENU_APPEND_PAGE),n().handleConversionRequest({tabId:t.id,caller:o().web2pdfCaller.MENU,action:o().web2pdfAction.APPEND,context:o().web2pdfContext.PAGE,url:e.pageUrl||t.url,domtitle:s(t)}))},documentUrlPatterns:i,id:"appendPageContextMenu"}),chrome.contextMenus.create({title:l("web2pdfConvertLinkContextMenu"),contexts:["link"],onclick:function(e,t){E(e,t)&&(acom_analytics.event(acom_analytics.e.CONTEXT_MENU_CONVERT_LINK),n().handleConversionRequest({tabId:t.id,caller:o().web2pdfCaller.MENU,action:o().web2pdfAction.CONVERT,context:o().web2pdfContext.LINK,url:e.linkUrl,domtitle:s(t)}))},documentUrlPatterns:i}),chrome.contextMenus.create({title:l("web2pdfAppendLinkContextMenu"),contexts:["link"],onclick:function(e,t){E(e,t)&&(acom_analytics.event(acom_analytics.e.CONTEXT_MENU_APPEND_LINK),n().handleConversionRequest({tabId:t.id,caller:o().web2pdfCaller.MENU,action:o().web2pdfAction.APPEND,context:o().web2pdfContext.LINK,url:e.linkUrl,domtitle:s(t)}))},documentUrlPatterns:i})):SETTINGS.IS_READER||("Adobe PDF",t=chrome.contextMenus.create({title:"Adobe PDF",contexts:r,id:"pdf-page"}),chrome.contextMenus.create({title:"Upload PDF to acrobat.com",contexts:r,parentId:t,id:"upload",documentUrlPatterns:a}),chrome.contextMenus.create({title:"Upload and export to Word/Excel/PowerPoint/Images",contexts:r,parentId:t,id:"export",documentUrlPatterns:a}),chrome.contextMenus.create({title:"Upload link to acrobat.com",contexts:["link"],parentId:t,id:"upload_link",targetUrlPatterns:c}),chrome.contextMenus.create({title:"Upload image to acrobat.com",contexts:["image"],parentId:t,id:"upload-image"}),chrome.contextMenus.create({title:"Create a Slideshow from a Flickr album",contexts:r,parentId:t,id:"flickr-slideshow",documentUrlPatterns:["*://www.flickr.com/*"]}),chrome.contextMenus.create({title:"Create a contact sheet from Flickr images",contexts:r,parentId:t,id:"flickr-contact-sheet",documentUrlPatterns:["*://www.flickr.com/*"]})))}startupComplete=!1,SETTINGS=SETTINGS||{USE_ACROBAT:!0},chrome.runtime.getPlatformInfo((function(e){"use strict";SETTINGS.OS=e.os})),require(["communicate","util","upload","download-manager","analytics","acro-gstate","acro-actions","floodgate","acro-web2pdf","session","convert-to-zip"],(function(e,t,n,o,a,i,r,c,l){"use strict";function s(n=!1,o){const a=t.isEdge(),i=SETTINGS.IS_ACROBAT&&!e.legacyShim(),r=SETTINGS.IS_BETA,c=!a&&SETTINGS.VIEWER_ENABLED&&n,l=function(){const e=chrome.i18n.getMessage("@@ui_locale"),t={ca:"en",eu:"en",cs:"cz",da:"dk",de:"de",en:"en",en_US:"en",en_GB:"en",es:"es",fi:"fi",fr:"fr",hr:"hr",hu:"hu",it:"it",ja:"jp",ko:"kr",nb:"no",nl:"nl",pl:"pl",pt:"pt",ro:"ro",ru:"ru",sk:"sk",sl:"sl",sv:"se",tr:"tr",uk:"ua",zh_CN:"cn",zh_TW:"tw"};try{return t[e]||"en"}catch(e){return"en"}}(),s="https://www.adobe.com/go/chrome_ext_landing";let E=null;if(a)return"https://documentcloud.adobe.com/dc-chrome-extension/Acrobat-for-Edge.pdf";if("normal"===o)return i?"https://www.adobe.com/go/chrome_ext_landing_pro_uk":"en"!==l?s+"_"+l:s;if(r)E="/acrobat/kb/acrobat-pro-chrome-extension-beta.html";else{if(c)return"https://documentcloud.adobe.com/dc-chrome-extension/Acrobat-for-Chrome.pdf";E=i?"/acrobat/kb/acrobat-pro-chrome-extension.html":"/acrobat/kb/acrobat-reader-chrome-extension.html"}return"https://helpx.adobe.com/"+l+E}function E(e){try{t.getCookie("pdfViewer")||(t.setCookie("fte","false"),t.setCookie("pdfViewer","true"),t.isEdge()?e.event(e.e.USE_ACROBAT_IN_EDGE_AUTO_ENABLED):e.event(e.e.USE_ACROBAT_IN_CHROME_AUTO_ENABLED))}catch(t){e.event(e.e.LOCAL_STORAGE_DISABLED)}}function d(e){return utilities&&utilities.isChromeOnlyMessage(e)&&utilities.isEdge()&&(e+="Edge"),utilities&&utilities.getTranslation?utilities.getTranslation(e):chrome.i18n.getMessage(e)}chrome.management.getSelf((function(e){!function(){try{0==localStorage.length&&""!=document.cookie&&document.cookie.split(/; */).map(e=>e.split("=")).filter(e=>e&&2==e.length).forEach(e=>localStorage.setItem(e[0],e[1]))}catch(e){}}(),function(){try{t.isEdge()&&localStorage.setItem("IsRunningInEdge",!0)}catch(e){}}(),a.s||a.init(e.version,e.installType);chrome.tabs.query({active:!0},(function(e){if(e.length>0&&(e[0].url.startsWith("https://chrome.google.com/webstore/detail/")||e[0].url.startsWith("https://microsoftedge.microsoft.com/addons/detail/"))&&-1!=e[0].url.indexOf("/"+chrome.runtime.id)){const t=new URLSearchParams(new URL(e[0].url).search);t.has("mv")&&(mvTracking=encodeURIComponent(t.get("mv")))}chrome.runtime.lastError&&(mvTracking=null)})),r.getVersion((function(n,o){n!==SETTINGS.READER_VER&&n!==SETTINGS.ERP_READER_VER||(SETTINGS.IS_READER=!0,SETTINGS.IS_ACROBAT=!1,n===SETTINGS.ERP_READER_VER&&(SETTINGS.IS_ERP_READER=!0),n===SETTINGS.ERP_READER_VER?chrome.browserAction.setTitle({title:d("web2pdfConvertButtonToolTipERPReader")}):chrome.browserAction.setTitle({title:d("web2pdfOpenButtonText")}),installSource=o),registerActions(),function(e){(0==e||1==e&&0==t.getNMHConnectionStatus()||e==SETTINGS.READER_VER||e==SETTINGS.ERP_READER_VER)&&chrome.contextMenus.removeAll()}(n),function(e){0!=e&&1!=e&&e!=SETTINGS.READER_VER&&e!=SETTINGS.ERP_READER_VER||chrome.browserAction.setTitle({title:""})}(n),started(a),chrome.storage.managed.get("OpenHelpx",(function(n){const o=!n||"false"!==n.OpenHelpx;a.event(o?a.e.VIEWER_FTE_OPEN_HELPX_ENABLED:a.e.VIEWER_FTE_OPEN_HELPX_DISABLED),function(e,n,o){"false"!==t.getCookie("fte")&&(t.isEdge()||"normal"===n?(t.setCookie("fte","false",3650),E(e),o&&t.createTab(s(!1,n))):c.getReleaseVariant("dc-cv-fte-experiments").then(a=>{const i="dc-cv-fte-helpx-staticpdf"===a;i&&(E(e),e.event(e.e.FTE_EXPERIMENT_STATIC_PDF),t.getCookie("staticFteCoachmarkShown")||t.setCookie("staticFteCoachmarkShown","false")),t.setCookie("fte","false",3650),o&&setTimeout(()=>t.createTab(s(i,n),(function(){i||chrome.tabs.onUpdated.addListener((function n(o,i,r){"complete"==i.status&&("dc-cv-fte-helpx-center"===a?e.event(e.e.VIEWER_FTE_CENTER_CARD):"dc-cv-fte-helpx-animated"===a?e.event(e.e.VIEWER_FTE_ANIMATED):e.event(e.e.VIEWER_FTE_LAUNCH),chrome.tabs.sendMessage(o,{fte_op:"FTE",panel_op:"FTE",fteExperiment:a,is_edge:t.isEdge()}),chrome.tabs.onUpdated.removeListener(n))}))})),2e3)}))}(a,e.installType,o)}))}))})),acom_analytics=a,communicate=e,utilities=t,SETTINGS.USE_ACROBAT||chrome.contextMenus.onClicked.addListener((function(e,t){var n={filename:t.title,tabId:t.id,menuItem:e.menuItemId,handleResult:"preview"};if("flickr-slideshow"===e.menuItemId||"flickr-contact-sheet"===e.menuItemId)return a.event(n,a.e.FLICKR_CONTEXT_CLICK),void communicate.deferMessage({panel_op:"flickr",tabId:t.id});"upload-image"===e.menuItemId&&(a.setOp("Image"),n.handleResult="image_preview",n.url=e.srcUrl),"upload_link"===e.menuItemId&&(a.setOp("Link"),n.url=e.linkUrl),"upload"===e.menuItemId&&(a.setOp("Link"),n.url=e.linkUrl),"pdf-page"===e.menuItemId&&(a.setOp("PdfPage"),n.url=e.pageUrl),n.filename.length>20&&(n.filename=n.filename.substring(0,19)),e.linkUrl?n.filename=e.linkUrl.split("/").splice(-1)[0].replace(/\?\S*/,""):e.srcUrl&&(n.url=e.srcUrl,n.filename=e.srcUrl.split("/").splice(-1)[0].replace(/\?\S*/,"")),"export"===e.menuItemId&&(n.handleResult="export"),o.proxy(o.do_upload(n))}))})),chrome.runtime.onInstalled.addListener(registerActions);