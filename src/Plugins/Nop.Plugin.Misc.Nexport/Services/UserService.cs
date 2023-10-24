using NexportApi.Model;
using Nop.Core.Domain.Common;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Services.Common;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IUserService
{
    Task<GetUserResponse?> AuthenticateUser(string username, string password);
    Task<CreateUserResponse?> CreateUser(string login, string password, string firstName, string lastName, string email, Guid ownerOrgId, UserContactInfoRequest? contactInfo = null);
    Task<GetUserResponse?> GetUser(Guid userId);
    Task<UserContactInfoResponse?> GetUserContactInfo(Guid userId);
    Task<EditUserResponse?> UpdateUserContactInfo(Guid userId, UserContactInfoRequest updatedInfo);
    Task<GetUserResponse?> ValidateUser(string username);
    Task CreateAndMapNewUserAsync(Customer customer);
    Task SynchronizeContactInfoFromAsync(Customer customer, Guid userId);
}

public class UserService : IUserService
{
    private readonly NexportApiService _nexportApi;
    private readonly HelperService _helper;
    private readonly IAddressService _address;
    private readonly IUserMappingService _userMapping;
    private readonly ICustomerService _customer;
    private readonly ICountryService _country;
    private readonly IStateProvinceService _stateProvince;

    public UserService(
        NexportApiService nexportApi,
        HelperService helper,
        IAddressService address,
        IUserMappingService userMapping,
        ICustomerService customer,
        ICountryService country,
        IStateProvinceService stateProvince)
    {
        _nexportApi = nexportApi;
        _helper = helper;
        _address = address;
        _userMapping = userMapping;
        _customer = customer;
        _country = country;
        _stateProvince = stateProvince;
    }

    public Task<GetUserResponse?> AuthenticateUser(string username, string password)
        => _helper.Do(async s => (await _nexportApi.AuthenticateUser((s.Url, s.Token), username, password)).Data);

    public Task<GetUserResponse?> ValidateUser(string username)
        => _helper.Do(async s => (await _nexportApi.GetUserByLogin((s.Url, s.Token), username)).Data);

    public Task<CreateUserResponse?> CreateUser(string login, string password, string firstName, string lastName, string email, Guid ownerOrgId, UserContactInfoRequest? contactInfo = null)
        => _helper.Do(async s => (await _nexportApi.CreateUser((s.Url, s.Token), (login, password), (firstName, lastName), email, ownerOrgId, contactInfo)).Data);

    public Task<GetUserResponse?> GetUser(Guid userId)
        => _helper.Do(async s => (await _nexportApi.GetUserByUserId((s.Url, s.Token), userId)).Data);

    public Task<UserContactInfoResponse?> GetUserContactInfo(Guid userId)
        => _helper.Do(async s => (await _nexportApi.GetUserContactInfo((s.Url, s.Token), userId)).Data);

    public Task<EditUserResponse?> UpdateUserContactInfo(Guid userId, UserContactInfoRequest updatedInfo)
        => _helper.Do(async s => (await _nexportApi.EditUserContactInfo((s.Url, s.Token), userId, updatedInfo)).Data);

    public async Task CreateAndMapNewUserAsync(Customer customer)
    {
        if (!_helper.IsValid() || await _userMapping.FindByCustomerId(customer.Id) is not null)
        {
            return;
        }
        var login = Guid.NewGuid().ToString();
        var password = CommonHelper.GenerateRandomDigitCode(20);
        UserContactInfoRequest? contactInfo = null;
        if (customer.BillingAddressId is not null
            && await _address.GetAddressByIdAsync(customer.BillingAddressId.Value) is { } currentBillingAddress)
        {
            var customerStateProvince = await _stateProvince.GetStateProvinceByIdAsync(currentBillingAddress.StateProvinceId ?? 0);
            var customerAddressState = customerStateProvince is not null ? customerStateProvince.Name : string.Empty;
            var customerCountry = await _country.GetCountryByIdAsync(currentBillingAddress.CountryId ?? 0);
            var customerAddressCountry = customerCountry is not null ? customerCountry.Name : string.Empty;
            contactInfo = new(apiErrorEntity: new ApiErrorEntity())
            {
                AddressLine1 = currentBillingAddress.Address1,
                AddressLine2 = currentBillingAddress.Address2,
                City = currentBillingAddress.City,
                State = customerAddressState,
                Country = customerAddressCountry,
                PostalCode = currentBillingAddress.ZipPostalCode,
                Phone = currentBillingAddress.PhoneNumber,
                Fax = currentBillingAddress.FaxNumber,
            };
        }
        if (await CreateUser(login, password, customer.FirstName, customer.LastName, customer.Email, _helper.RootOrganizationId.Value, contactInfo) is not { } response)
        {
            return;
        }
        var userMapping = new UserMapping
        {
            UserId = response.UserId,
            NopUserId = customer.Id,
        };
        await _userMapping.InsertUserMapping(userMapping);
    }

    public async Task SynchronizeContactInfoFromAsync(Customer customer, Guid userId)
    {
        if (await GetUserContactInfo(userId) is not { } userContactInfo)
        {
            return;
        }
        var customerStateProvince = (await _stateProvince.GetStateProvincesAsync())
            .FirstOrDefault(state =>
                state.Name.Contains(userContactInfo.State, StringComparison.OrdinalIgnoreCase)
                    || state.Abbreviation.Contains(userContactInfo.State, StringComparison.OrdinalIgnoreCase));
        var customerCountry = (await _country.GetAllCountriesAsync())
            .FirstOrDefault(country =>
                country.Name.Contains(userContactInfo.Country, StringComparison.OrdinalIgnoreCase)
                    || country.ThreeLetterIsoCode.Contains(userContactInfo.Country, StringComparison.OrdinalIgnoreCase)
                    || country.TwoLetterIsoCode.Contains(userContactInfo.Country, StringComparison.OrdinalIgnoreCase));
        var address = new Address
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Address1 = userContactInfo.AddressLine1,
            Address2 = userContactInfo.AddressLine2,
            City = userContactInfo.City,
            StateProvinceId = customerStateProvince?.Id,
            CountryId = customerCountry?.Id,
            ZipPostalCode = userContactInfo.PostalCode,
            Email = customer.Email,
            PhoneNumber = userContactInfo.Phone,
            FaxNumber = userContactInfo.Fax,
            CreatedOnUtc = DateTime.UtcNow,
        };
        if (address.CountryId is 0)
        {
            address.CountryId = null;
        }
        if (address.StateProvinceId is 0)
        {
            address.StateProvinceId = null;
        }
        await _customer.InsertCustomerAddressAsync(customer, address);
        customer.BillingAddressId = address.Id;
        customer.ShippingAddressId = address.Id;
        await _customer.UpdateCustomerAsync(customer);
    }
}
