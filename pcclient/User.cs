using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pcclient
{
    class User
    {
        private long userId;

        public long UserId
        {
            get { return userId; }
            set { userId = value; }
        }
        private String nickname;

        public String Nickname
        {
            get { return nickname; }
            set { nickname = value; }
        }
        private String email;

        public String Email
        {
            get { return email; }
            set { email = value; }
        }
        private String sex;

        public String Sex
        {
            get { return sex; }
            set { sex = value; }
        }
        private String headImage;

        public String HeadImage
        {
            get { return headImage; }
            set { headImage = value; }
        }
        private String company;

        public String Company
        {
            get { return company; }
            set { company = value; }
        }
        private String position;

        public String Position
        {
            get { return position; }
            set { position = value; }
        }

    }
}
