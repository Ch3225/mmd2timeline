using System.Collections.Generic;

namespace mmd2timeline
{
    // 基于 miscreated.BodyHandling.75 MuscleManager 的身体部位控制轴定义
    public class BodyPartControlMapping
    {
        public class ControlAxis
        {
            public string Label;
            public float Min;
            public float Max;
            public float Default;
            
            public ControlAxis(string label, float min, float max, float defaultValue = 0f)
            {
                Label = label;
                Min = min;
                Max = max;
                Default = defaultValue;
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
            
            public void AddAxis(string label, float min, float max, float defaultValue = 0f)
            {
                Axes.Add(new ControlAxis(label, min, max, defaultValue));
            }
        }
        
        public static Dictionary<string, BodyPartControls> GetBodyPartMapping()
        {
            var mapping = new Dictionary<string, BodyPartControls>();
            
            // Head - 3 axes
            var head = new BodyPartControls("Head");
            head.AddAxis("Fwd ↔ Back", -35f, 45f);
            head.AddAxis("Turn Left ↔ Right", -35f, 35f);
            head.AddAxis("Lean Left ↔ Right", -30f, 30f);
            mapping["head"] = head;
            
            // Left Shoulder - 3 axes
            var lCollar = new BodyPartControls("Left Shoulder");
            lCollar.AddAxis("Down ↔ Up", -15f, 50f);
            lCollar.AddAxis("Forw ↔ Back", -20f, 20f);
            lCollar.AddAxis("Rotate", -15f, 15f);
            mapping["lCollar"] = lCollar;
            
            // Right Shoulder - 3 axes
            var rCollar = new BodyPartControls("Right Shoulder");
            rCollar.AddAxis("Down ↔ Up", -15f, 50f);
            rCollar.AddAxis("Forw ↔ Back", -20f, 20f);
            rCollar.AddAxis("Rotate", -15f, 15f);
            mapping["rCollar"] = rCollar;
            
            // Left Arm - 3 axes
            var lShldr = new BodyPartControls("Left Arm");
            lShldr.AddAxis("Down ↔ Up", -75f, 30f);
            lShldr.AddAxis("Forw ↔ Back", -80f, 80f);
            lShldr.AddAxis("Twist In ↔ Out", -70f, 70f);
            mapping["lShldr"] = lShldr;
            
            // Right Arm - 3 axes
            var rShldr = new BodyPartControls("Right Arm");
            rShldr.AddAxis("Down ↔ Up", -75f, 30f);
            rShldr.AddAxis("Forw ↔ Back", -80f, 80f);
            rShldr.AddAxis("Twist In ↔ Out", -70f, 70f);
            mapping["rShldr"] = rShldr;
            
            // Left Elbow - 2 axes
            var lForeArm = new BodyPartControls("Left Elbow");
            lForeArm.AddAxis("Bend ↔ Straight", -130f, 20f);
            lForeArm.AddAxis("Twist Y", -65f, 65f);
            mapping["lForeArm"] = lForeArm;
            
            // Right Elbow - 2 axes
            var rForeArm = new BodyPartControls("Right Elbow");
            rForeArm.AddAxis("Bend ↔ Straight", -130f, 20f);
            rForeArm.AddAxis("Twist Y", -65f, 65f);
            mapping["rForeArm"] = rForeArm;
            
            // Left Hand - 3 axes
            var lHand = new BodyPartControls("Left Hand");
            lHand.AddAxis("In ↔ Out", -80f, 80f);
            lHand.AddAxis("Twist Y", -40f, 40f);
            lHand.AddAxis("Twist Z", -40f, 40f);
            mapping["lHand"] = lHand;
            
            // Right Hand - 3 axes
            var rHand = new BodyPartControls("Right Hand");
            rHand.AddAxis("In ↔ Out", -80f, 80f);
            rHand.AddAxis("Twist Y", -40f, 40f);
            rHand.AddAxis("Twist Z", -40f, 40f);
            mapping["rHand"] = rHand;
            
            // Spine - 3 axes (compound control)
            var chest = new BodyPartControls("Spine");
            chest.AddAxis("Forw ↔ Back", -50f, 50f);
            chest.AddAxis("Twist Right↔Left", -40f, 40f);
            chest.AddAxis("Twist Z", -40f, 40f);
            mapping["chest"] = chest;
            
            // Chest - 3 axes
            var chestOnly = new BodyPartControls("Chest");
            chestOnly.AddAxis("Forw ↔ Back", -20f, 20f);
            chestOnly.AddAxis("Twist Right↔Left", -20f, 20f);
            chestOnly.AddAxis("Twist Z", -25f, 25f);
            mapping["Chest"] = chestOnly;
            
            // Abd L - 3 axes
            var abdL = new BodyPartControls("Abd L");
            abdL.AddAxis("Forw ↔ Back", -30f, 20f);
            abdL.AddAxis("Twist Right↔Left", -15f, 15f);
            abdL.AddAxis("Twist Z", -10f, 10f);
            mapping["Abd L"] = abdL;
            
            // Abd H - 3 axes
            var abdH = new BodyPartControls("Abd H");
            abdH.AddAxis("Forw ↔ Back", -30f, 20f);
            abdH.AddAxis("Twist Right↔Left", -15f, 15f);
            abdH.AddAxis("Twist Z", -10f, 10f);
            mapping["Abd H"] = abdH;
            
            // Pelvis - 3 axes
            var pelvis = new BodyPartControls("Pelvis");
            pelvis.AddAxis("Forw ↔ Back", -30f, 15f);
            pelvis.AddAxis("Twist Right↔Left", -15f, 15f);
            pelvis.AddAxis("Twist Z", -10f, 10f);
            mapping["pelvis"] = pelvis;
            
            // Left Thigh - 3 axes (这是关键！)
            var lThigh = new BodyPartControls("Left Thigh");
            lThigh.AddAxis("Back ↔ Forw", -25f, 100f);
            lThigh.AddAxis("Toes In ↔ Out", -75f, 75f);
            lThigh.AddAxis("Straight ↔ Spread", -85f, 85f);
            mapping["lThigh"] = lThigh;
            
            // Right Thigh - 3 axes (这是关键！)
            var rThigh = new BodyPartControls("Right Thigh");
            rThigh.AddAxis("Back ↔ Forw", -25f, 100f);
            rThigh.AddAxis("Toes In ↔ Out", -75f, 75f);
            rThigh.AddAxis("Straight ↔ Spread", -85f, 85f);
            mapping["rThigh"] = rThigh;
            
            // Left Knee - 1 axis
            var lShin = new BodyPartControls("Left Knee");
            lShin.AddAxis("Bend ↔ Straight", -150f, 11f);
            mapping["lShin"] = lShin;
            
            // Right Knee - 1 axis
            var rShin = new BodyPartControls("Right Knee");
            rShin.AddAxis("Bend ↔ Straight", -150f, 11f);
            mapping["rShin"] = rShin;
            
            // Left Foot - 3 axes
            var lFoot = new BodyPartControls("Left Foot");
            lFoot.AddAxis("Toe ↔ Heal", -65f, 40f);
            lFoot.AddAxis("Toes In ↔ Out", -20f, 20f);
            lFoot.AddAxis("Lean In ↔ Out", -20f, 20f);
            mapping["lFoot"] = lFoot;
            
            // Right Foot - 3 axes
            var rFoot = new BodyPartControls("Right Foot");
            rFoot.AddAxis("Toe ↔ Heal", -65f, 40f);
            rFoot.AddAxis("Toes In ↔ Out", -20f, 20f);
            rFoot.AddAxis("Lean In ↔ Out", -20f, 20f);
            mapping["rFoot"] = rFoot;
            
            // Left Toe - 1 axis
            var lToe = new BodyPartControls("Left Toe");
            lToe.AddAxis("Down ↔ Up", -65f, 75f);
            mapping["lToe"] = lToe;
            
            // Right Toe - 1 axis
            var rToe = new BodyPartControls("Right Toe");
            rToe.AddAxis("Down ↔ Up", -65f, 75f);
            mapping["rToe"] = rToe;
            
            return mapping;
        }
    }
}