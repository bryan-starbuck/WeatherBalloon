using System;
using System.Collections.Generic;


namespace WeatherBalloon.BalloonModule
{
    /// <summary>
    /// Random id generator based on how Docker names containers
    /// </summary>
    public class FlightIdGenerator 
    {
        public static List<string> adjectives = new List<string> {
            "Adorable","Delightful","Homely","Quant","Adventurous","Depressed","Horrible","Aggressive","Determined","Hungry","Real","Agreeable","Different","Hurt","Relieved",
            "Alert","Difficult","Repulsive","Alive","Disgusted","Ill","Rich","Amused","Distinct","Important","Angry","Disturbed","Impossible","Scary","Annoyed","Dizzy","Inexpensive","Selfish",
            "Annoying","Doubtful","Innocent","Shiny","Anxious","Drab","Inquisitive","Shy","Arrogant","Dull","Itchy","Silly","Ashamed","Sleepy","Attractive","Eager","Jealous","Smiling","Average","Easy","Jittery","Smoggy",
            "Awful","Elated","Jolly","Sore","Elegant","Joyous","Sparkling","Bad","Embarrassed","Splendid","Beautiful","Enchanting","Kind","Spotless",
            "Better","Encouraging","Stormy","Bewildered","Energetic","Lazy","Strange","Black","Enthusiastic","Light","Stupid","Bloody","Envious","Lively","Successful",
            "Blue","Evil","Lonely","Super","Blue-eyed","Excited","Long","Blushing","Expensive","Lovely","Talented",
            "Bored","Exuberant","Lucky","Tame","Brainy","Tender","Brave","Fair","Magificent","Tense","Breakable","Faithful","Misty","Terrible","Bright","Famous","Modern","Tasty",
            "Busy","Fancy","Motionless","Thankful","Fantastic","Muddy","Thoughtful",
            "Calm","Fierce","Mushy","Thoughtless","Careful","Filthy","Mysterious","Tired"
        };

        public static List<string> nouns = new List<string> {
            "aardvark","abacus","abalone","abdomen","absinthe","airplane","albacore","albino","album","backstage","backstop","backstroke","backup","backward","backyard","bacon","campground","campus","can",
            "candle","candlelight","candlestick","candy","cane","cheetah","chef","chemist","cherry","chess","chessboard","dandelion","dandruff","dart","dartboard","dashboard","dealer","ear",
            "earache","earphone","earplug","earring","fender","fennel","fern","ferret","ferry","ferryboat","garbage","garden","gardener","gargoyle","garlic","goalie","goalkeeper","goalpost","goat","goatee",
            "hammer","hammock","hamper","hamster","horse","horsefly","horseshoe","hose","hospital","invoice","iron",
            "italic","ivory","ivy","knickknack","knife","knight","knitter","knitting","knob","lineup","link","lion","lioness",
            "magnesium","magnet","magnolia","matchmaker","matchmaking","mate","material","nest","net","network","neurologist","oat","oatmeal","oats",
            "panda","pane","panel","panther","pantry","quartet","queen","query","quest",
            "race","racehorse","racer","racetrack","saltwater","samaritan","sample","sampler"
        };

        public static string Generate ()
        {
            var random = new Random();

            return $"{adjectives[(int)Math.Floor((adjectives.Count-1) * random.NextDouble())]} {nouns[(int)Math.Floor((nouns.Count -1)  * random.NextDouble())]}";
        }

    }

}
