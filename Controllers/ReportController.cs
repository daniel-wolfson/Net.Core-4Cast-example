﻿using Crpm.Dal;
using Crpm.Dal.Services;
using Crpm.Infrastructure.Auth;
using Crpm.Infrastructure.Helpers;
using Crpm.Model.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crpm.Controllers
{
    [Route("api/DalApi/[controller]")]
    [EnableCors(PolicyTypes.ApiCorsPolicy)]
    [ApiController]
    public class ReportController : ControllerBaseAction
    {
        private readonly ReportService _reportService;
        private readonly OrganizationService _orgService;

        public ReportController(ReportService report, OrganizationService organization)
        {
            _reportService = report;
            _orgService = organization;
        }

        [HttpPost("FillTreeScores")]
        public async Task<IActionResult> FillTreeScores([FromBody] Dictionary<string, object> data)
        {
            var refCalcItm = data["item1"] as CalculateData;
            var copyFlatModel = data["item2"] as Dictionary<string, CalculateTreeData>;

            if (refCalcItm == null || copyFlatModel == null)
                return Ok(null);

            string model_component_guid = refCalcItm.model_component_guid;
            OrganizationObjectDataInfo org_obj = refCalcItm.org_obj;
            DateTime? calculate_date = refCalcItm.calculate_date;
            List<string> activities = refCalcItm.activity_list;
            var candidateGuid = refCalcItm.candidatesList?.Count > 0 ? refCalcItm.candidatesList[0] : null;

            Dictionary<string, CalculateTreeData> result = await _reportService.FillTreeScores(refCalcItm.report_guid,
                org_obj, copyFlatModel, activities, calculate_date, candidateGuid);

            return Ok(result);
        }

        [HttpPost("FillTreeScoresRef")]
        public async Task<IActionResult> FillTreeScoresRef([FromBody] Dictionary<string, object> data)
        {
            var refCalcItm = data["refCalcItm"] as CalculateData;
            var orgModels = data["orgModels"] as Dictionary<string, List<OrgModels>>;
            var copyFlatModel = data["copyFlatModel"] as Dictionary<string, CalculateTreeData>;

            if (refCalcItm == null || orgModels == null || copyFlatModel == null)
                return Ok(null);

            string model_component_guid = refCalcItm.model_component_guid;
            OrganizationObjectDataInfo org_obj = refCalcItm.org_obj;
            DateTime? calculate_date = refCalcItm.calculate_date;
            List<string> activities = refCalcItm.activity_list;
            var candidateGuid = refCalcItm.candidatesList?.Count > 0 ? refCalcItm.candidatesList[0] : null;

            Dictionary<string, CalculateTreeData> result = await _reportService.FillTreeScoresRef(model_component_guid, org_obj, orgModels, copyFlatModel, activities,
                calculate_date, candidateGuid);

            return Ok(result);
        }

        [HttpPost("SaveCalculateTree")]
        public async Task<IActionResult> SaveCalculateTree([FromBody] Dictionary<string, object> data)
        {
            CalculateTreeData calculate_tree = data["Tree"] as CalculateTreeData;

            bool result = await _reportService.SaveCalculateScores(calculate_tree.report_guid, calculate_tree);
            return await _reportService.OkResult(result);
        }

        [HttpPost("GetScoreLevel")]
        public async Task<IActionResult> GetScoreLevel([FromQuery] double? score, [FromQuery] int? metric_measuring_unit, [FromBody] List<ConvertionTableData> convertion_table)
        {
            int result = await _reportService.GetScoreLevelValue(score, metric_measuring_unit, convertion_table);
            return Ok(result);
        }

        [HttpPost("UpdateCalculateTree")]
        public async Task<IActionResult> UpdateCalculateTree([FromBody] Dictionary<string, object> data)
        {
            CalculateTreeData tree = data["Tree"] as CalculateTreeData;
            CalculateData calc_data = data["Data"] as CalculateData;

            bool hasReference = tree.children.Exists(x => x.data.model_data.is_reference);

            CalculateTreeData result = (await _reportService.UpdateCalculateTree(calc_data.report_guid, calc_data.report_type, new List<CalculateTreeData>() { tree }, calc_data.calculate_date, calc_data.org_obj, calc_data.comment_list, calc_data.focus_list, true, hasReference))[0];

            return Ok(result);
        }

        [HttpPost("SaveReportDataAsJson")]
        public async Task<IActionResult> SaveReportDataAsJson([FromBody] dynamic data)
        {
            data = JsonConvert.DeserializeObject<dynamic>(Util.Decompress(data));

            if (data == null)
                return null;

            string result = await _reportService.SaveReportAsJson(data);
            return await _reportService.OkResult(result);
        }

        [HttpGet("GetReportByGuid")]
        public async Task<IActionResult> GetReportByGuid([FromQuery] string report_guid)
        {
            List<string> result = await _reportService.GetReportByGuid(report_guid);
            return Ok(result);
        }

        [HttpGet("GetUserPrimaryReport")]
        public async Task<IActionResult> GetUserPrimaryReport([FromQuery] string user_guid)
        {
            string result = await _reportService.GetUserPrimaryReport(user_guid);
            return Ok(result);
        }
        [HttpGet("GetUserHybridReports")]
        public async Task<IActionResult> GetUserHybridReports([FromQuery] string user_guid)
        {
            var result = await _reportService.GetUserHybridReports(user_guid);
            return Ok(result);
        }




        [HttpGet("GetUserSecondaryReport")]
        public async Task<IActionResult> GetUserSecondaryReport([FromQuery] string user_guid)
        {
            string result = await _reportService.GetUserSecondaryReport(user_guid);
            return Ok(result);
        }

        [HttpGet("GetUserWatchReports")]
        public async Task<IActionResult> GetUserWatchReports([FromQuery] string user_guid)
        {
            List<WatchData> result = await _reportService.GetUserWatchReports(user_guid);
            return Ok(result);
        }

        [HttpGet("GetUserReports")]
        public async Task<IActionResult> GetUserReports([FromQuery] string user_guid)
        {
            List<SavedReportDataInfo> result = await _reportService.GetUserReports(user_guid);
            return Ok(result);
        }

        [HttpGet("GetSavedReport")]
        public async Task<IActionResult> GetSavedReport([FromQuery] string report_guid)
        {
            SavedReportDataInfo result = await _reportService.GetSavedReport(report_guid);
            return Ok(result);
        }

        [HttpPost("SaveReport")]
        public async Task<IActionResult> SaveReport([FromBody] ReportData data)
        {
            string result = await _reportService.SaveReport(data);
            return await _reportService.OkResult(result);
        }

        [HttpGet("ToggleReportViewType")]
        public async Task<IActionResult> ToggleReportViewType([FromQuery] string report_guid, [FromQuery] ReportView exist_report_view, [FromQuery] int order)
        {
            bool result = await _reportService.ToggleReportViewType(report_guid, exist_report_view, order);
            return await _reportService.OkResult(result);
        }

        [HttpGet("DeleteSavedReport")]
        public async Task<IActionResult> DeleteSavedReport([FromQuery] string report_guid)
        {
            bool result = await _reportService.DeleteSavedReport(report_guid);
            return await _reportService.OkResult(result);
        }

        [HttpPost("GetOrgObjModels")]
        public async Task<IActionResult> GetOrgObjModels([FromBody] string[] data)
        {
            var result = await _reportService.GetOrgObjModels(data);
            return Ok(result);
        }

        #region InterFace
        [HttpPost("GetCandidateByOrgGuid")]
        public async Task<IActionResult> GetCandidateByOrgGuid([FromBody] List<string> data)
        {
            var result = await _reportService.GetCandidateByOrgGuid(data);
            return this.Ok(result);
        }
        [HttpPost("GetOrgObjModelsByUserHR")]
        public async Task<IActionResult> GetOrgObjModelsByUserHR([FromBody] List<string> data)
        {
            var result = await _reportService.GetOrgObjModelsByUserHR(data);
            return this.Ok(result);
        }
        #endregion
    }
}
