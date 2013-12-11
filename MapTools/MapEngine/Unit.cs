using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEngine
{
    [Serializable]
    public class Unit : ICloneable
    {
        public string UnitType { get; set; }
        public string UnitAIType { get; set; }
        public int UnitOwner { get; set; }
        public int Damage { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int FacingDirection { get; set; }
        public List<string> UnparsedData { get; set; }

        public Unit()
        {
            UnparsedData = new List<string>();
        }


        #region ICloneable Members

        public object Clone()
        {
            Unit u = (Unit)MemberwiseClone();
            u.UnparsedData = new List<string>(UnparsedData);
            return u;
        }

        #endregion
    }

}
