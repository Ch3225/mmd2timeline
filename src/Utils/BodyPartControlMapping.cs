using System.Collections.Generic;

namespace mmd2timeline
{
    // 基于 miscreated.BodyHandling.75 MuscleManager 的身体部位控制轴定义
    public class BodyPartControlMapping
    {
        public class ControlAxis
        {
            public string Label;
            public string Axis;  // X, Y, Z
            public float Min;
            public float Max;
            public float Default;
            public int Neg = 1;  // 对称性乘数，用于左右对称部位
            
            public ControlAxis(string label, string axis, float min, float max, float defaultValue = 0f, int neg = 1)
            {
                Label = label;
                Axis = axis;
                Min = min;
                Max = max;
                Default = defaultValue;
                Neg = neg;
            }
        }
        
        public class BodyPartControls
        {
            public string Name;
            public List<ControlAxis> Axes = new List<ControlAxis>();
            
            public BodyPartControls(string name)
            {
                Name = name;
            }
            
            public void AddAxis(string label, string axis, float min, float max, float defaultValue = 0f, int neg = 1)
            {
                Axes.Add(new ControlAxis(label, axis, min, max, defaultValue, neg));
            }
        }
        
        public static Dictionary<string, BodyPartControls> GetBodyPartMapping()
        {
            var mapping = new Dictionary<string, BodyPartControls>();
            
            // Head - 3 axes
            var head = new BodyPartControls("Head");
            head.AddAxis("Fwd ↔ Back", "X", -35f, 45f);
            head.AddAxis("Turn Left ↔ Right", "Y", -35f, 35f);
            head.AddAxis("Lean Left ↔ Right", "Z", -30f, 30f);
            mapping["head"] = head;
            
            // Left Shoulder - 3 axes
            var lCollar = new BodyPartControls("Left Shoulder");
            lCollar.AddAxis("Down ↔ Up", "X", -15f, 50f, 0f, 1);      // X轴正常
            lCollar.AddAxis("Forw ↔ Back", "Y", -20f, 20f, 0f, 1);     // Y轴正常  
            lCollar.AddAxis("Rotate", "Z", -15f, 15f, 0f, 1);          // Z轴正常
            mapping["lCollar"] = lCollar;
            
            // Right Shoulder - 3 axes - 关键：右肩需要镜像
            var rCollar = new BodyPartControls("Right Shoulder");
            rCollar.AddAxis("Down ↔ Up", "X", -15f, 50f, 0f, -1);     // X轴镜像
            rCollar.AddAxis("Forw ↔ Back", "Y", -20f, 20f, 0f, -1);    // Y轴镜像
            rCollar.AddAxis("Rotate", "Z", -15f, 15f, 0f, 1);          // Z轴正常
            mapping["rCollar"] = rCollar;
            
            // Left Arm - 3 axes
            var lShldr = new BodyPartControls("Left Arm");
            lShldr.AddAxis("Down ↔ Up", "X", -75f, 30f, 0f, 1);       // X轴正常
            lShldr.AddAxis("Forw ↔ Back", "Y", -80f, 80f, 0f, 1);      // Y轴正常
            lShldr.AddAxis("Twist In ↔ Out", "Z", -70f, 70f, 0f, 1);   // Z轴正常
            mapping["lShldr"] = lShldr;
            
            // Right Arm - 3 axes - 关键：右臂需要镜像
            var rShldr = new BodyPartControls("Right Arm");
            rShldr.AddAxis("Down ↔ Up", "X", -75f, 30f, 0f, -1);      // X轴镜像
            rShldr.AddAxis("Forw ↔ Back", "Y", -80f, 80f, 0f, 1);      // Y轴正常
            rShldr.AddAxis("Twist In ↔ Out", "Z", -70f, 70f, 0f, 1);   // Z轴正常
            mapping["rShldr"] = rShldr;
            
            // Left Elbow - 2 axes
            var lForeArm = new BodyPartControls("Left Elbow");
            lForeArm.AddAxis("Bend ↔ Straight", "X", -130f, 20f, 0f, 1);
            lForeArm.AddAxis("Twist Y", "Y", -65f, 65f, 0f, 1);        // 根据参考插件，肘部Y轴不镜像
            mapping["lForeArm"] = lForeArm;
            
            // Right Elbow - 2 axes - 关键：右肘Y轴不镜像！
            var rForeArm = new BodyPartControls("Right Elbow");
            rForeArm.AddAxis("Bend ↔ Straight", "X", -130f, 20f, 0f, -1);  // X轴镜像
            rForeArm.AddAxis("Twist Y", "Y", -65f, 65f, 0f, 1);         // Y轴不镜像（这是修复！）
            mapping["rForeArm"] = rForeArm;
            
            // Left Hand - 3 axes
            var lHand = new BodyPartControls("Left Hand");
            lHand.AddAxis("In ↔ Out", "X", -80f, 80f, 0f, 1);
            lHand.AddAxis("Twist Y", "Y", -40f, 40f, 0f, 1);
            lHand.AddAxis("Twist Z", "Z", -40f, 40f, 0f, 1);
            mapping["lHand"] = lHand;
            
            // Right Hand - 3 axes - 关键：右手需要Y轴镜像
            var rHand = new BodyPartControls("Right Hand");
            rHand.AddAxis("In ↔ Out", "X", -80f, 80f, 0f, -1);        // X轴镜像
            rHand.AddAxis("Twist Y", "Y", -40f, 40f, 0f, -1);          // Y轴镜像
            rHand.AddAxis("Twist Z", "Z", -40f, 40f, 0f, 1);           // Z轴正常
            mapping["rHand"] = rHand;
            
            // Spine - 3 axes (compound control)
            var chest = new BodyPartControls("Spine");
            chest.AddAxis("Forw ↔ Back", "X", -50f, 50f);
            chest.AddAxis("Twist Right↔Left", "Y", -40f, 40f);
            chest.AddAxis("Twist Z", "Z", -40f, 40f);
            mapping["chest"] = chest;
            
            // Pelvis - 3 axes
            var pelvis = new BodyPartControls("Pelvis");
            pelvis.AddAxis("Forw ↔ Back", "X", -30f, 15f);
            pelvis.AddAxis("Twist Right↔Left", "Y", -15f, 15f);
            pelvis.AddAxis("Twist Z", "Z", -10f, 10f);
            mapping["pelvis"] = pelvis;
            
            // Left Thigh - 3 axes
            var lThigh = new BodyPartControls("Left Thigh");
            lThigh.AddAxis("Back ↔ Forw", "X", -25f, 100f, 0f, 1);
            lThigh.AddAxis("Toes In ↔ Out", "Y", -75f, 75f, 0f, 1);
            lThigh.AddAxis("Straight ↔ Spread", "Z", -85f, 85f, 0f, 1);
            mapping["lThigh"] = lThigh;
            
            // Right Thigh - 3 axes - 关键：右大腿需要Y和Z轴镜像
            var rThigh = new BodyPartControls("Right Thigh");
            rThigh.AddAxis("Back ↔ Forw", "X", -25f, 100f, 0f, 1);      // X轴正常
            rThigh.AddAxis("Toes In ↔ Out", "Y", -75f, 75f, 0f, -1);    // Y轴镜像
            rThigh.AddAxis("Straight ↔ Spread", "Z", -85f, 85f, 0f, -1); // Z轴镜像
            mapping["rThigh"] = rThigh;
            
            // Left Knee - 1 axis
            var lShin = new BodyPartControls("Left Knee");
            lShin.AddAxis("Bend ↔ Straight", "X", -150f, 11f);
            mapping["lShin"] = lShin;
            
            // Right Knee - 1 axis
            var rShin = new BodyPartControls("Right Knee");
            rShin.AddAxis("Bend ↔ Straight", "X", -150f, 11f, 0f, -1);  // X轴镜像
            mapping["rShin"] = rShin;
            
            // Left Foot - 3 axes
            var lFoot = new BodyPartControls("Left Foot");
            lFoot.AddAxis("Toe ↔ Heal", "X", -65f, 40f, 0f, 1);
            lFoot.AddAxis("Toes In ↔ Out", "Y", -20f, 20f, 0f, 1);
            lFoot.AddAxis("Lean In ↔ Out", "Z", -20f, 20f, 0f, 1);
            mapping["lFoot"] = lFoot;
            
            // Right Foot - 3 axes - 关键：右脚需要Y和Z轴镜像
            var rFoot = new BodyPartControls("Right Foot");
            rFoot.AddAxis("Toe ↔ Heal", "X", -65f, 40f, 0f, 1);        // X轴正常
            rFoot.AddAxis("Toes In ↔ Out", "Y", -20f, 20f, 0f, -1);     // Y轴镜像
            rFoot.AddAxis("Lean In ↔ Out", "Z", -20f, 20f, 0f, -1);     // Z轴镜像
            mapping["rFoot"] = rFoot;
            
            // Left Toe - 1 axis
            var lToe = new BodyPartControls("Left Toe");
            lToe.AddAxis("Down ↔ Up", "X", -65f, 75f);
            mapping["lToe"] = lToe;
            
            // Right Toe - 1 axis
            var rToe = new BodyPartControls("Right Toe");
            rToe.AddAxis("Down ↔ Up", "X", -65f, 75f, 0f, -1);         // X轴镜像
            mapping["rToe"] = rToe;
            
            return mapping;
        }
    }
}