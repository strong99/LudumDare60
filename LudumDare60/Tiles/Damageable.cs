using System;
using System.Collections.Generic;
using System.Text;

namespace LudumDare60.Tiles;

public interface Damageable {
    Single HitPoints { get; set; }
    Int32 MaxHitPoints { get; set; }
}
