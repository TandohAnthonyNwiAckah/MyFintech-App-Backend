// AutoMapping.cs
using AutoMapper;
using TANA.Models;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        CreateMap<BankaccountAddDTO, BankAccount>();
        CreateMap<BankaccountEditDTO, BankAccount>();
        CreateMap<BeneficiariesAddDTO, Beneficiaries>();
        CreateMap<BeneficiariesEditDTO, Beneficiaries>();
        CreateMap<EmploymentAddDTO, Employment>();
        CreateMap<EmploymentEditDTO, Employment>();
        CreateMap<InvestmentnameAddDTO, InvestmentName>();
        CreateMap<InvestmentnameEditDTO, InvestmentName>();
        CreateMap<InvestmentsAddDTO, Investments>();
        CreateMap<InvestmentsEditDTO, Investments>();
        CreateMap<PermissionsAddDTO, Permissions>();
        CreateMap<PermissionsEditDTO, Permissions>();
        CreateMap<ProductnameAddDTO, ProductName>();
        CreateMap<ProductnameEditDTO, ProductName>();
        CreateMap<RolesAddDTO, Roles>();
        CreateMap<RolesEditDTO, Roles>();
        CreateMap<UsersRegisterDTO, Users>();
        CreateMap<UsersAccounteditDTO, Users>();
        CreateMap<UsersAddDTO, Users>();
        CreateMap<UsersEditDTO, Users>();
        CreateMap<CustomersAddDTO, Customers>();
        CreateMap<CustomersEditDTO, Customers>();
    }
}
