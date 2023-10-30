﻿namespace Mbill.Service.Bill.Asset.Input;

public class CreateAssetInput
{
    /// <summary>
    /// 资产分类名
    /// </summary>
    [Required(ErrorMessage = "必须传入资产分类名称")]
    public string Name { get; set; }

    /// <summary>
    /// 父级BId
    /// </summary>
    public long ParentBId { get; set; }

    /// <summary>
    /// 资产分类类型：存款、负债
    /// </summary>
    [Required(ErrorMessage = "必须传入资产分类类型")]
    public int Type { get; set; }

    /// <summary>
    /// 资产金额
    /// </summary>

    public decimal Amount { get; set; }

    /// <summary>
    /// 图标地址
    /// </summary>

    public string Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
}
