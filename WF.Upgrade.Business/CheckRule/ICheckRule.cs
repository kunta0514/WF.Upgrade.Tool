using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Business
{
    public interface ICheckRule
    {
        /// <summary>
        /// 检测接口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        CheckResult Check(object input);

        /// <summary>
        /// 修复接口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        object Repair(object input);

        /// <summary>
        /// 规则名称
        /// </summary>
        //string CheckRuleName { get;  }
    }

    
}
