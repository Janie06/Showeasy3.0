using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Hub
{
    public class RoomInfo
    {
        [Key]
        public string RoomName { get; set; }

        //用户集合
        public virtual List<UserInfo> Users { get; set; }

        public RoomInfo()
        {
            Users = new List<UserInfo>();
        }
    }
}