using AutoMapper;
using Pet.API.Models.Entities;
using Pet.API.Models.DTOs;
using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Pet mappings
            CreateMap<CreatePetRequest, PetEntity>()
                .ForMember(dest => dest.PetId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Available"))
                .ForMember(dest => dest.IntakeDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

            CreateMap<UpdatePetRequest, PetEntity>()
                .ForMember(dest => dest.PetId, opt => opt.Ignore())
                .ForMember(dest => dest.IntakeDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<PetEntity, PetResponse>();

            // Adoption mappings
            CreateMap<CreateAdoptionRequest, Adoption>()
                .ForMember(dest => dest.AdoptionId, opt => opt.Ignore())
                .ForMember(dest => dest.PetName, opt => opt.Ignore()) // Will be set by controller
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.ApplicationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.ReviewedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewNotes, opt => opt.Ignore());

            CreateMap<UpdateAdoptionRequest, Adoption>()
                .ForMember(dest => dest.AdoptionId, opt => opt.Ignore())
                .ForMember(dest => dest.PetId, opt => opt.Ignore())
                .ForMember(dest => dest.PetName, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.UserEmail, opt => opt.Ignore())
                .ForMember(dest => dest.UserFirstName, opt => opt.Ignore())
                .ForMember(dest => dest.UserLastName, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationDate, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewNotes, opt => opt.Ignore());

            CreateMap<Adoption, AdoptionResponse>();

            // Medical Record mappings
            CreateMap<CreateMedicalRecordRequest, MedicalRecord>()
                .ForMember(dest => dest.RecordId, opt => opt.Ignore())
                .ForMember(dest => dest.PetName, opt => opt.Ignore()) // Will be set by controller
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

            CreateMap<UpdateMedicalRecordRequest, MedicalRecord>()
                .ForMember(dest => dest.RecordId, opt => opt.Ignore())
                .ForMember(dest => dest.PetName, opt => opt.Ignore()) // Will be set by controller
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<MedicalRecord, MedicalRecordResponse>();
        }
    }
}

