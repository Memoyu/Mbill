﻿namespace mbill.Service.Bill.Bill;

public class BillSvc : ApplicationSvc, IBillSvc
{
    private readonly IBillRepo _billRepo;
    private readonly IPreOrderRepo _preOrderRepo;
    private readonly ICategoryRepo _categoryRepo;
    private readonly IAssetRepo _assetRepo;
    private readonly IFileRepo _fileRepo;

    public BillSvc(
        IBillRepo billRepo,
        IPreOrderRepo preOrderRepo,
        ICategoryRepo categoryRepo,
        IAssetRepo assetRepo,
        IFileRepo fileRepo)
    {
        _billRepo = billRepo;
        _preOrderRepo = preOrderRepo;
        _categoryRepo = categoryRepo;
        _assetRepo = assetRepo;
        _fileRepo = fileRepo;
    }

    public async Task<ServiceResult<BillSimpleDto>> CreateAsync(ModifyBillInput input)
    {
        var bill = Mapper.Map<BillEntity>(input);
        var entity = await _billRepo.InsertAsync(bill);
        if (entity == null) ServiceResult<BillSimpleDto>.Failed("新增账单失败！");
        return ServiceResult<BillSimpleDto>.Successed(await MapToSimpleDto(entity));
    }

    public async Task<ServiceResult> DeleteAsync(long id)
    {
        var exist = await _billRepo.Select.AnyAsync(s => s.Id == id && !s.IsDeleted);
        if (!exist) return ServiceResult.Failed("没有找到该账单信息");
        var re = await _billRepo.DeleteAsync(id);
        return ServiceResult.Successed("删除成功");
    }

    public async Task<ServiceResult<BillSimpleDto>> UpdateAsync(ModifyBillInput input)
    {
        var bill = Mapper.Map<BillEntity>(input);
        var exist = await _billRepo.Select.AnyAsync(s => s.Id == bill.Id && !s.IsDeleted);
        if (!exist) return ServiceResult<BillSimpleDto>.Failed("没有找到该账单信息");
        Expression<Func<BillEntity, object>> ignoreExp = e => new { e.CreateUserId, e.CreateTime };
        await _billRepo.UpdateWithIgnoreAsync(bill, ignoreExp);
        return ServiceResult<BillSimpleDto>.Successed(await MapToSimpleDto(bill));
    }

    public async Task<ServiceResult<BillDetailDto>> GetDetailAsync(long id)
    {
        var bill = await _billRepo.GetAsync(id);
        if (bill == null)
            return ServiceResult<BillDetailDto>.Failed("没有找到该账单信息");
        var dto = Mapper.Map<BillDetailDto>(bill);
        var category = await _categoryRepo.GetAsync(bill.CategoryId.Value);
        var assetDto = await _assetRepo.GetAssetAsync(dto.AssetId);
        dto.Asset = assetDto?.Name;
        dto.Category = category?.Name;
        dto.CategoryIcon = _fileRepo.GetFileUrl(category?.IconUrl);
        return ServiceResult<BillDetailDto>.Successed(dto);
    }

    public async Task<ServiceResult<BillsByDayDto>> GetByDayAsync(DayBillInput input)
    {
        var begin = input.Date.Date;
        var end = begin.AddDays(1).AddSeconds(-1);
        var bills = await _billRepo
            .Select
            .Where(s => s.IsDeleted == false)
            .Where(s => s.CreateUserId == CurrentUser.Id)
            .Where(s => s.Time >= begin && s.Time <= end)
            .WhereIf(input.Type.HasValue, s => s.Type == input.Type)
            .WhereIf(input.CategoryId.HasValue, s => s.CategoryId == input.CategoryId)
            .WhereIf(input.AssetId.HasValue, s => s.AssetId == input.AssetId)
            .OrderBy("time DESC")
            .ToListAsync();

        var dto = new BillsByDayDto();
        dto.Day = begin.Day;
        dto.Week = begin.GetWeek();
        foreach (var i in bills)
        {
            dto.Items.Add(await MapToSimpleDto(i));
        }
        return ServiceResult<BillsByDayDto>.Successed(dto);
    }

    public async Task<ServiceResult<PagedDto<BillsByDayDto>>> GetByMonthPagesAsync(MonthBillPagingInput input)
    {
        input.Sort = input.Sort.IsNullOrEmpty() ? "time DESC" : input.Sort.Replace("-", " ");
        var bills = await _billRepo
            .Select
            .Where(s => s.IsDeleted == false)
            .Where(s => s.CreateUserId == CurrentUser.Id)
            .Where(s => s.Time.Year == input.Month.Year && s.Time.Month == input.Month.Month)
            .WhereIf(input.Type.HasValue, s => s.Type == input.Type)
            .WhereIf(input.CategoryId.HasValue, s => s.CategoryId == input.CategoryId)
            .WhereIf(input.AssetId.HasValue, s => s.AssetId == input.AssetId)
            .OrderBy(input.Sort)
            .ToPageListAsync(input, out long totalCount);

        List<BillsByDayDto> dtos = new List<BillsByDayDto>();
        bills.GroupBy(b => b.Time.Date).ForEach(async b =>
        {
            var dto = new BillsByDayDto();
            dto.Day = b.Key.Day;
            dto.Week = b.Key.GetWeek();
            foreach (var i in b)
            {
                dto.Items.Add(await MapToSimpleDto(i));
            }
            dtos.Add(dto);
        });

        return ServiceResult<PagedDto<BillsByDayDto>>.Successed(new PagedDto<BillsByDayDto>(dtos, totalCount));
    }

    public async Task<ServiceResult<List<BillDateWithTotalDto>>> RangeHasBillDaysAsync(RangeHasBillDaysInput input)
    {
        var begin = input.BeginDate.FirstDayOfMonth();
        var end = input.EndDate.LastDayOfMonth().AddDays(1).AddSeconds(-1);
        var bills = await _billRepo
            .Select
            .Where(s => s.IsDeleted == false)
            .Where(s => s.CreateUserId == CurrentUser.Id)
            .Where(s => s.Time <= end && s.Time >= begin)
            .ToListAsync();
        var dtos = bills.GroupBy(b => b.Time.Date).Select(
            g => new BillDateWithTotalDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Day = g.Key.Day,
                Total = g.Count(),
            }).ToList();
        return ServiceResult<List<BillDateWithTotalDto>>.Successed(dtos);
    }

    public async Task<ServiceResult<MonthTotalStatDto>> GetMonthTotalStatAsync(MonthTotalStatInput input)
    {
        var begin = input.Month.FirstDayOfMonth();
        var end = input.Month.LastDayOfMonth().AddDays(1).AddSeconds(-1);
        var bills = await _billRepo
               .Select
               .Where(s => s.IsDeleted == false)
               .Where(s => s.CreateUserId == CurrentUser.Id)
               .Where(s => s.Time <= end && s.Time >= begin)
               .ToListAsync();

        var expend = 0m;
        var income = 0m;
        bills.ForEach(b =>
        {
            if (b.Type == (int)BillTypeEnum.expend)
            {
                expend += b.Amount;
            }
            else if (b.Type == (int)BillTypeEnum.income)
            {
                income += b.Amount;
            }
        });

        return ServiceResult<MonthTotalStatDto>.Successed(new MonthTotalStatDto
        {
            Expend = expend.AmountFormat(),
            Income = income.AmountFormat()
        });
    }

    public async Task<ServiceResult<YearTotalStatDto>> GetYearTotalStatAsync(YearTotalStatInput input)
    {
        var bills = await _billRepo
               .Select
               .Where(s => s.IsDeleted == false)
               .Where(s => s.CreateUserId == CurrentUser.Id)
               .Where(s => s.Time.Year == input.Year)
               .ToListAsync();
        var preOrders = await _preOrderRepo
           .Select
           .Where(s => s.IsDeleted == false)
           .Where(s => s.CreateUserId == CurrentUser.Id)
           .Where(s => s.Time.Year == input.Year)
           .ToListAsync();

        var expend = 0m;
        var income = 0m;
        var preOrder = 0m;
        bills.ForEach(b =>
        {
            if (b.Type == (int)BillTypeEnum.expend)
            {
                expend += b.Amount;
            }
            else if (b.Type == (int)BillTypeEnum.income)
            {
                income += b.Amount;
            }
        });
        preOrders.ForEach(b =>
        {
            preOrder += b.Amount;
        });

        return ServiceResult<YearTotalStatDto>.Successed(new YearTotalStatDto
        {
            Expend = expend.AmountFormat(),
            Income = income.AmountFormat(),
            PreOrder = preOrder.AmountFormat(),
        });
    }

    public async Task<BillTotalDto> GetStatisticsTotalAsync(BillDateInput input)
    {
        // var userId = input.UserId ?? CurrentUser.Id;
        var bills = await _billRepo
           .Select
           .Where(s => s.IsDeleted == false)
           .WhereIf(input.UserId != null, s => s.CreateUserId == input.UserId)
           //.WhereIf(input.Year != null, s => s.Year == input.Year)
           //.WhereIf(input.Month != null, s => s.Month == input.Month)
           .ToListAsync();
        var dto = new BillTotalDto();
        bills.ForEach(s =>
        {
            switch (s.Type)
            {
                //case "expend":
                //    dto.MonthExpend += s.Amount;
                //    break;
                //case "income":
                //    dto.MonthIcome += s.Amount;
                //    break;
                //case "repayment":
                //    dto.MonthRepayment += s.Amount;
                //    break;
                //case "transfer":
                //    dto.MonthTransfer += s.Amount;
                //    break;
            }
            //if (input.Day != null && input.Day == s.Day)
            //{
            //    switch (s.Type)
            //    {
            //        case "expend":
            //            dto.DayExpend += s.Amount;
            //            break;
            //        case "income":
            //            dto.DayIcome += s.Amount;
            //            break;
            //        case "repayment":
            //            dto.DayRepayment += s.Amount;
            //            break;
            //        case "transfer":
            //            dto.DayTransfer += s.Amount;
            //            break;
            //    }
            //}
        });
        return dto;
    }

    public async Task<BillExpendCategoryDto> GetExpendCategoryStatisticsAsync(BillDateInput input)
    {
        var dto = new BillExpendCategoryDto();
        var bills = await _billRepo
           .Select
           .Where(s => s.IsDeleted == false)
           .Where(s => s.Type.Equals("expend"))
           .WhereIf(input.UserId != null, s => s.CreateUserId == input.UserId)
           //.WhereIf(input.Year != null, s => s.Year == input.Year)
           //.WhereIf(input.Month != null, s => s.Month == input.Month)
           //.WhereIf(input.Day != null, s => s.Day == input.Day)
           .ToListAsync();
        decimal total = 0;
        // 根据CategoryId分组，并统计总额
        var childDetails = bills.GroupBy(s => s.CategoryId).Select(g =>
        {
            var info = _categoryRepo.GetAsync(g.Key.Value).Result;
            var parentInfo = _categoryRepo.GetCategoryParentAsync(g.Key.Value).Result;
            var amount = g.Sum(s => s.Amount);
            total += amount;
            return new
            {
                CategoryId = g.Key,
                Amount = amount,
                Info = info,
                ParentInfo = parentInfo
            };
        });

        var childDtos = new List<ChildGroupDto>();
        var parentDtos = childDetails.GroupBy(p => new { p.ParentInfo.Id, p.ParentInfo.Name }).Select(g =>
        {
            var childDto = new ChildGroupDto();
            var childTotal = g.Sum(s => s.Amount);
            childDto.ParentName = g.Key.Name;
            childDto.Childs = childDetails.Where(d => d.Info.ParentId == g.Key.Id).Select(d => new
            {
                Id = d.Info.Id,
                Name = d.Info.Name,
                Data = d.Amount,
                Percent = Math.Round(d.Amount / childTotal, 4) * 100,
                CategoryIconPath = _fileRepo.GetFileUrl(d.Info.IconUrl)
            });
            childDtos.Add(childDto);
            return new StatisticsDto
            {
                Id = g.Key.Id,
                Name = g.Key.Name,
                Data = Math.Round(g.Sum(s => s.Amount) / total, 4) * 100
            };
        }).ToList();
        dto.ParentCategoryStas = parentDtos;
        dto.ChildCategoryStas = childDtos;
        return dto;
    }

    public async Task<IEnumerable<BillExpendTrendDto>> GetWeekExpendTrendStatisticsAsync(BillDateInput input)
    {

        var dateList = DateTimeUtil.GetWeeksOfMonth(input.Year.Value, input.Month.Value);
        var startDate = dateList.OrderBy(d => d.Number).FirstOrDefault().StartDate.Date;
        var EndDate = dateList.OrderBy(d => d.Number).LastOrDefault().EndDate.Date.AddDays(1).AddSeconds(-1);// 加上23:59:59

        var bills = await _billRepo
          .Select
          .Where(s => s.IsDeleted == false)
          //.WhereIf(input.Type.IsNotNullOrEmpty(), s => s.Type == input.Type)
          .WhereIf(input.UserId != null, s => s.CreateUserId == input.UserId)
          //.WhereIf(input.Year != null, s => s.Year == input.Year)
          .WhereIf(input.Month != null, s => s.Time >= startDate && s.Time <= EndDate)
          .ToListAsync();

        var dtos = dateList.Select(d => new BillExpendTrendDto
        {
            Name = $"{d.StartDate.Month}/{d.StartDate.Day}-{d.EndDate.Month}/{d.EndDate.Day}",
            Data = bills.Where(s => s.Time >= d.StartDate && s.Time <= d.EndDate.AddDays(1).AddSeconds(-1)).Select(t => new { t.Amount }).Sum(t => t.Amount),
            StartDate = d.StartDate.Date,
            EndDate = d.EndDate.Date
        });

        return dtos;
    }

    public async Task<IEnumerable<BillExpendTrendDto>> GetMonthExpendTrendStatisticsAsync(BillDateInput input, int count)
    {
        var dateList = new List<WeeksOfMonth>();
        // 获取当前月份前count个月份（包含当前月份）
        for (int i = 0; i < count; i++)
        {
            var currentDate = new DateTime(input.Year.Value, input.Month.Value, 1).AddMonths(-i);
            dateList.Add(new WeeksOfMonth
            {
                Number = i + 1,
                StartDate = currentDate,
                EndDate = currentDate.AddMonths(1).AddDays(-1)
            });
        }

        var startDate = dateList.OrderBy(d => d.Number).LastOrDefault().StartDate.Date;//获取最小的月份（Number最大）
        var EndDate = dateList.OrderBy(d => d.Number).FirstOrDefault().EndDate.Date;

        var bills = await _billRepo
         .Select
         .Where(s => s.IsDeleted == false)
         // .WhereIf(input.Type.IsNotNullOrEmpty(), s => s.Type == input.Type)
         .WhereIf(input.UserId != null, s => s.CreateUserId == input.UserId)
         // .WhereIf(input.Year != null, s => s.Year >= startDate.Year && s.Year <= EndDate.Year)
         .WhereIf(input.Month != null, s => s.Time >= startDate && s.Time <= EndDate.AddDays(1).AddSeconds(-1))
         .ToListAsync();

        var dtos = dateList.Select(d => new BillExpendTrendDto
        {
            Name = $"{d.StartDate.Year}/{d.StartDate.Month}",
            // Data = bills.Where(s => s.Year == d.StartDate.Year && s.Month == d.StartDate.Month).Select(t => new { t.Amount }).Sum(t => t.Amount),
            StartDate = d.StartDate.Date,
            EndDate = d.EndDate.Date
        });

        return dtos;


    }

    #region Private

    /// <summary>
    /// 映射Dto
    /// </summary>
    /// <param name="bill"></param>
    /// <returns></returns>
    private async Task<BillSimpleDto> MapToSimpleDto(BillEntity bill)
    {
        var dto = Mapper.Map<BillSimpleDto>(bill);
        dto.Time = bill.Time.ToString("HH:mm");
        var category = await _categoryRepo.GetAsync(bill.CategoryId.Value);
        dto.Category = category?.Name;
        dto.CategoryIcon = _fileRepo.GetFileUrl(category?.IconUrl);
        return dto;
    }

    #endregion

}
