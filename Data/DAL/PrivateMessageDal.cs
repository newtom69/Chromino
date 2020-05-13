﻿using Data.Enumeration;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.DAL
{
    public class PrivateMessageDal
    {
        private readonly Context Ctx;
        public PrivateMessageDal(Context context)
        {
            Ctx = context;
        }

        public DateTime GetDateLatestReadMessage(int recipientId, int senderId)
        {
            DateTime result = (from pm in Ctx.PrivatesMessagesLatestRead
                               where pm.RecipientId == recipientId && pm.SenderId == senderId
                               select pm.LatestRead).FirstOrDefault();

            return result;
        }

        public void SetDateLatestReadMessage(int recipientId, int senderId, DateTime date)
        {
            var privateMessageLatestRead = (from pm in Ctx.PrivatesMessagesLatestRead
                                            where pm.RecipientId == recipientId && pm.SenderId == senderId
                                            select pm).FirstOrDefault();

            if (privateMessageLatestRead != null)
                privateMessageLatestRead.LatestRead = date;
            else
                Ctx.PrivatesMessagesLatestRead.Add(new PrivateMessageLatestRead { RecipientId = recipientId, SenderId = senderId, LatestRead = DateTime.Now });
            Ctx.SaveChanges();
        }

        public void Add(int playerId, int opponentId, string message)
        {
            PrivateMessage pm = new PrivateMessage { SenderId = playerId, RecipientId = opponentId, Date = DateTime.Now, Message = message };
            Ctx.PrivatesMessages.Add(pm);
            Ctx.SaveChanges();
        }

        public List<PrivateMessage> GetMessages(int player1Id, int player2Id, DateTime dateMin, DateTime dateMax)
        {
            var result = (from pm in Ctx.PrivatesMessages
                          where (pm.RecipientId == player1Id && pm.SenderId == player2Id || pm.RecipientId == player2Id && pm.SenderId == player1Id) && pm.Date > dateMin && pm.Date <= dateMax
                          select pm).ToList();

            return result;
        }

        public int NewMessagesNumber(int player1Id, int player2Id, DateTime dateMin)
        {
            var result = (from pm in Ctx.PrivatesMessages
                          where (pm.RecipientId == player1Id && pm.SenderId == player2Id || pm.RecipientId == player2Id && pm.SenderId == player1Id) && pm.Date > dateMin
                          select pm).Count();

            return result;
        }
        public Dictionary<int, int> GetUnreadMessagesSenders(int playerId)
        {
            Dictionary<int, int> result = (from pm in Ctx.PrivatesMessages
                                           join lr in Ctx.PrivatesMessagesLatestRead on pm.RecipientId equals lr.RecipientId
                                           where pm.RecipientId == playerId && pm.SenderId == lr.SenderId && pm.Date > lr.LatestRead
                                           group pm by pm.SenderId into g
                                           select new { SenderId = g.Key, MessagesNumber = g.Count() }).ToDictionary(x => x.SenderId, x => x.MessagesNumber);

            return result;
        }
    }
}