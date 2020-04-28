using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AutoMapper;

namespace AdvertApi.Services
{
    public class DynamoDbAdvertStorageService : IAdvertStorageService
    {
        private readonly IMapper _mapper;
        public DynamoDbAdvertStorageService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map<AdvertDbModel>(model);

            dbModel.Id = new Guid().ToString();
            dbModel.CreateDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var client = new AmazonDynamoDBClient())
            {
                using(var context = new DynamoDBContext(client))
                {
                    await context.SaveAsync(dbModel);
                }
            }

            return dbModel.Id;
        }

        public async Task<bool> CheckHealthAsync()
        {
            using (var client = new AmazonDynamoDBClient())
            {
                var tableData = await client.DescribeTableAsync("Adverts");

                return string.Compare(tableData.Table.TableStatus, "active", true) == 0;
            }
        }

        public async Task Confirm(ConfirmAdvertModel model)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var dbRecord = await context.LoadAsync<AdvertDbModel>(model.Id);
                    if (dbRecord == null)
                    {
                        var msg = $"A record with ID of '{model.Id}' was not found";
                        throw new KeyNotFoundException(msg);
                    }

                    if (model.Status == AdvertStatus.Active)
                    {
                        dbRecord.Status = AdvertStatus.Active;
                        await context.SaveAsync(dbRecord);
                    } 
                    else
                    {
                        await context.DeleteAsync(dbRecord);
                    }
                }
            }
        }

        public async Task<AdvertModel> Get(Guid id)
        {
            AdvertModel dbModel;
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var dbRecord = await context.LoadAsync<AdvertDbModel>(id);
                    if (dbRecord == null)
                    {
                        var msg = $"A record with ID of '{id}' was not found";
                        throw new KeyNotFoundException(msg);
                    }
                    dbModel = _mapper.Map<AdvertModel>(dbRecord);
                }
            }
            return dbModel;
        }
    }
}
