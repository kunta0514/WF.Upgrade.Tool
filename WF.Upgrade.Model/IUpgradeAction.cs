using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WF.Upgrade.Model
{
    public interface IUpgradeAction
    {
        UpgradeActionResult Action(object input);
    }
}
