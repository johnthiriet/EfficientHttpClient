using System.Collections.Generic;

namespace EfficientApiCalls
{
    public class Name
    {
        public string first { get; set; }
        public string last { get; set; }
    }

    public class Friend
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Model
    {
        public string _id { get; set; }
        public int index { get; set; }
        public string guid { get; set; }
        public bool isActive { get; set; }
        public string balance { get; set; }
        public string picture { get; set; }
        public int age { get; set; }
        public string eyeColor { get; set; }
        public Name name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string about { get; set; }
        public string registered { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public List<string> tags { get; set; }
        public List<int> range { get; set; }
        public List<Friend> friends { get; set; }
        public string greeting { get; set; }
        public string favoriteFruit { get; set; }
    }
}
