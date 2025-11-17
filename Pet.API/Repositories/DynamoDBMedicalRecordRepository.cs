using Amazon.DynamoDBv2.Model;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;

namespace Pet.API.Repositories
{
    public class DynamoDBMedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly string _medicalRecordsTableName;

        public DynamoDBMedicalRecordRepository(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
            _medicalRecordsTableName = dynamoDBContext.MedicalRecordsTableName;
        }

        public async Task<MedicalRecord> CreateAsync(MedicalRecord record)
        {
            if (string.IsNullOrEmpty(record.RecordId))
            {
                record.RecordId = $"medical-{Guid.NewGuid():N}";
            }

            record.CreatedDate = DateTime.UtcNow;
            record.UpdatedDate = null;

            var item = new Dictionary<string, AttributeValue>
            {
                { "RecordId", new AttributeValue { S = record.RecordId } },
                { "PetId", new AttributeValue { S = record.PetId } },
                { "PetName", new AttributeValue { S = record.PetName } },
                { "RecordType", new AttributeValue { S = record.RecordType } },
                { "RecordDate", new AttributeValue { S = record.RecordDate.ToString("o") } },
                { "VeterinarianId", new AttributeValue { S = record.VeterinarianId } },
                { "VeterinarianName", new AttributeValue { S = record.VeterinarianName } },
                { "Description", new AttributeValue { S = record.Description } },
                { "VaccineName", new AttributeValue { S = record.VaccineName ?? "" } },
                { "Cost", new AttributeValue { N = record.Cost.ToString("F2") } },
                { "Notes", new AttributeValue { S = record.Notes ?? "" } },
                { "CreatedDate", new AttributeValue { S = record.CreatedDate.ToString("o") } }
            };

            if (record.NextDueDate.HasValue)
            {
                item["NextDueDate"] = new AttributeValue { S = record.NextDueDate.Value.ToString("o") };
            }

            var request = new PutItemRequest
            {
                TableName = _medicalRecordsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return record;
        }

        public async Task<MedicalRecord?> GetByIdAsync(string recordId)
        {
            var request = new GetItemRequest
            {
                TableName = _medicalRecordsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "RecordId", new AttributeValue { S = recordId } }
                }
            };

            var response = await _dynamoDBContext.Client.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapToMedicalRecord(response.Item);
        }

        public async Task<List<MedicalRecord>> GetByPetIdAsync(string petId)
        {
            var request = new ScanRequest
            {
                TableName = _medicalRecordsTableName,
                FilterExpression = "PetId = :petId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":petId", new AttributeValue { S = petId } }
                }
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var records = new List<MedicalRecord>();
            foreach (var item in response.Items)
            {
                records.Add(MapToMedicalRecord(item));
            }

            return records.OrderByDescending(r => r.RecordDate).ToList();
        }

        public async Task<List<MedicalRecord>> GetAllAsync()
        {
            var request = new ScanRequest
            {
                TableName = _medicalRecordsTableName
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var records = new List<MedicalRecord>();
            foreach (var item in response.Items)
            {
                records.Add(MapToMedicalRecord(item));
            }

            return records.OrderByDescending(r => r.RecordDate).ToList();
        }

        public async Task<MedicalRecord> UpdateAsync(MedicalRecord record)
        {
            record.UpdatedDate = DateTime.UtcNow;

            var item = new Dictionary<string, AttributeValue>
            {
                { "RecordId", new AttributeValue { S = record.RecordId } },
                { "PetId", new AttributeValue { S = record.PetId } },
                { "PetName", new AttributeValue { S = record.PetName } },
                { "RecordType", new AttributeValue { S = record.RecordType } },
                { "RecordDate", new AttributeValue { S = record.RecordDate.ToString("o") } },
                { "VeterinarianId", new AttributeValue { S = record.VeterinarianId } },
                { "VeterinarianName", new AttributeValue { S = record.VeterinarianName } },
                { "Description", new AttributeValue { S = record.Description } },
                { "VaccineName", new AttributeValue { S = record.VaccineName ?? "" } },
                { "Cost", new AttributeValue { N = record.Cost.ToString("F2") } },
                { "Notes", new AttributeValue { S = record.Notes ?? "" } },
                { "CreatedDate", new AttributeValue { S = record.CreatedDate.ToString("o") } },
                { "UpdatedDate", new AttributeValue { S = record.UpdatedDate.Value.ToString("o") } }
            };

            if (record.NextDueDate.HasValue)
            {
                item["NextDueDate"] = new AttributeValue { S = record.NextDueDate.Value.ToString("o") };
            }

            var request = new PutItemRequest
            {
                TableName = _medicalRecordsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return record;
        }

        public async Task DeleteAsync(string recordId)
        {
            var request = new DeleteItemRequest
            {
                TableName = _medicalRecordsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "RecordId", new AttributeValue { S = recordId } }
                }
            };

            await _dynamoDBContext.Client.DeleteItemAsync(request);
        }

        private MedicalRecord MapToMedicalRecord(Dictionary<string, AttributeValue> item)
        {
            var record = new MedicalRecord
            {
                RecordId = item.GetValueOrDefault("RecordId")?.S ?? "",
                PetId = item.GetValueOrDefault("PetId")?.S ?? "",
                PetName = item.GetValueOrDefault("PetName")?.S ?? "",
                RecordType = item.GetValueOrDefault("RecordType")?.S ?? "",
                VeterinarianId = item.GetValueOrDefault("VeterinarianId")?.S ?? "",
                VeterinarianName = item.GetValueOrDefault("VeterinarianName")?.S ?? "",
                Description = item.GetValueOrDefault("Description")?.S ?? "",
                VaccineName = item.GetValueOrDefault("VaccineName")?.S ?? "",
                Notes = item.GetValueOrDefault("Notes")?.S ?? ""
            };

            // Parse RecordDate
            if (item.GetValueOrDefault("RecordDate")?.S != null)
            {
                record.RecordDate = DateTime.Parse(item["RecordDate"].S);
            }

            // Parse NextDueDate
            if (item.GetValueOrDefault("NextDueDate")?.S != null)
            {
                record.NextDueDate = DateTime.Parse(item["NextDueDate"].S);
            }

            // Parse Cost
            if (item.GetValueOrDefault("Cost")?.N != null)
            {
                record.Cost = decimal.Parse(item["Cost"].N);
            }

            // Parse CreatedDate
            if (item.GetValueOrDefault("CreatedDate")?.S != null)
            {
                record.CreatedDate = DateTime.Parse(item["CreatedDate"].S);
            }

            // Parse UpdatedDate
            if (item.GetValueOrDefault("UpdatedDate")?.S != null)
            {
                record.UpdatedDate = DateTime.Parse(item["UpdatedDate"].S);
            }

            return record;
        }
    }
}

