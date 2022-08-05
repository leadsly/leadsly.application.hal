using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public class Failure
    {
        public Codes? Code { get; set; }
        public string ResourceId { get; set; }             
        public string Arn { get; set; }        
        public string Detail { get; set; }        
        public string Reason { get; set; }
    }

    public enum Codes
    {
        NONE,
        HTTP_REQUEST_ERROR,
        DESERIALIZATION_ERROR,
        CONFIGURATION_DATA_MISSING,
        OBJECT_MAPPING,
        HEALTHCHECK_FAILURE,
        AWS_API_ERROR,
        AWS_API_RESPONSE_MISSING_DATA,
        AWS_CLOUD_UNEXPECTED_STATE,  
        HAL_API_ERROR,
        HAL_INTERNAL_SERVER_ERROR,
        DATABASE_OPERATION_ERROR,
        ERROR,
        NOT_FOUND,
        CACHE_ITEM_NOT_FOUND,
        WEBDRIVER_ERROR,
        WEBDRIVER_MANAGEMENT_ERROR,
        WEBDRIVER_WINDOW_LOCATION_ERROR,
        WEBDRIVER_TWO_FA_CODE_ERROR,
        FILE_CLONING_ERROR
    }
}
