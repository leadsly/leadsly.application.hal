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
function dependOn(){"use strict";return[require("communicate"),require("util")]}var def;require=function(e){"use strict";return e},def=window.define?window.define:function(e,t){"use strict";return t.apply(null,[{ajax:$.ajax.bind($)}])};var exports=acom_analytics={};def(dependOn(),(function(e,t){function s(){let e;return t.getCookie("pdfViewer")?"true"===t.getCookie("pdfViewer")?e="enabled":"false"===t.getCookie("pdfViewer")&&(e="disabled"):e="neverEnabled",e}chrome.extension.onMessageExternal.addListener((function(t,r,a){"use strict";if(/^https:\/\/([a-zA-Z\d-]+\.){0,}(adobe|acrobat)\.com(:[0-9]*)?$/.test(r.origin))if("WebRequest"===t.type)switch(t.task){case"detect_extension":a({status:"success",result:"true"});break;case"detect_desktop":try{if(0!=e.version&&1!=e.version){a(e.version===SETTINGS.READER_VER||e.version===SETTINGS.ERP_READER_VER?{status:"success",result:"Reader"}:{status:"success",result:"Acrobat"})}else a({status:"success",result:"NoApp"})}catch(e){a({status:"error",code:"error"})}break;case"detect_viewer_enabled":try{a({status:"success",result:s()})}catch(e){a({status:"error",code:"error"})}break;default:a({status:"error",code:"invalid_task"})}else a({status:"error",code:"invalid_type"})}))}));