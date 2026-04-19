using Duende.IdentityServer.EntityFramework.DbContexts;
using ExchangeRateProvider.Admin.Models.Consumers;
using ExchangeRateProvider.Application.Consumers.Handlers.Admin;
using ExchangeRateProvider.Contract.Commons;
using ExchangeRateProvider.Contract.Commons.Options;
using ExchangeRateProvider.Contract.Consumers;
using ExchangeRateProvider.Contract.Consumers.Dtos.Requests;
using ExchangeRateProvider.Contract.Consumers.Dtos.Responses;
using ExchangeRateProvider.Contract.Consumers.Services;
using ExchangeRateProvider.Domain.Consumers.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeRateProvider.Admin.Controllers
{
    [Authorize(AuthenticationSchemes = "oidc")]
    public class ConsumerController(
        UserManager<User> userManager,
        IConsumerCommandRepository consumerCommand,
        IConsumerQueryRepository consumerQuery,
        IApiKeyClientService apiKeyClientService,
        IMarketTradingPairQueryRepository tradingPairQueryRepository,
        IProviderQueryRepository providerQuery,
        IMarketQueryRepository marketQuery,
        INotifier notifier,
        EncryptionConfiguration encryptionConfiguration,
        IRedisDatabase redisDatabase,
        ConfigurationDbContext configurationDbContext,
        ILogger<Consumer> logger) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> List(
            string projectName,
            string username,
            string isActive,
            int index = 1,
            RequestPagingSize size = RequestPagingSize.Ten,
            CancellationToken cancellationToken = default)
        {
            var filter = new ConsumerPaginatedFilterRequest
            {
                ProjectName = projectName,
                UserName = username,
                IsActive = string.IsNullOrEmpty(isActive) ? null : int.Parse(isActive) == 1,
            };

            var paging = new RequestPaging
            {
                Index = index,
                Size = size
            };

            var request = new PaginationRequest<ConsumerPaginatedFilterRequest>
            {
                Filter = filter,
                Paging = paging
            };

            var response = await consumerQuery.GetConsumersWithPaginationAndFilterAsync(request, cancellationToken);

            var model = new ListViewModel
            {
                PaginationResponse = response,
                SizeOptions = GetPageSizeOptions((int)size),
            };

            return View(model);
        }

        public IActionResult AddConsumer([FromQuery] string selectedUserId)
        {
            var informationModel = ConsumerHandlers.GetConsumerInformationAsync(userManager);
            var model = new CreateConsumerModel()
            {
                UserId = int.TryParse(selectedUserId, out var id) ? id : 0,
                Users = GenerateUserSelectList(informationModel.Users, [selectedUserId]),
                IsEditMode = false
            };

            return View(new ResponseWrapper<CreateConsumerModel>()
            {
                Response = model,
                IsSuccess = true
            });
        }

        [HttpPost]
        public async Task<ResponseWrapper<int>> AddConsumerInformation(
            [FromForm] ResponseWrapper<CreateConsumerModel> request,
            CancellationToken cancellationToken)
        {
            ValidateSpreadOption(request.Response.SpreadOptions);
            var result = await ConsumerHandlers.AddConsumerInformation(request.Response, consumerCommand, consumerQuery, userManager, logger, cancellationToken);

            result.Data.Add("redirectUrl", Url.Action("Edit", "Consumer"));
            result.Data.Add("message", "Consumer created successfully");

            return result;
        }

        [HttpPost]
        public IActionResult AddConsumer([FromForm] CreateConsumerModel model, [FromForm] string actionType,
            CancellationToken cancellationToken)
        {
            var result = new ResponseWrapper<CreateConsumerModel>()
            {
                Response = model,
                IsSuccess = true
            };

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string mode, CancellationToken cancellationToken)
        {
            var model = await ConsumerHandlers.GetConsumerDataForEdit(
                id,
                consumerQuery,
                tradingPairQueryRepository,
                providerQuery,
                marketQuery,
                logger,
                encryptionConfiguration.EncryptionKey,
                configurationDbContext,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(mode) && mode.Equals("Create"))
            {
                model.Response.IsEditMode = false;
                var secrets = await GenerateSecretAsync(model.Response.ConsumerId, cancellationToken);
                model.Response.ClientSecret = secrets.clientSecret;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ResponseWrapper<ConsumerResponse>> Edit([FromRoute] int id, ResponseWrapper<CreateConsumerModel> model,
            [FromForm] string actionType, CancellationToken cancellationToken)
        {
            ValidateSpreadOption(model.Response.SpreadOptions);
            model.Response.CreatorUserId = GetCurrentUserId();

            var result = await ConsumerHandlers.UpdateAsync(model.Response, model.Response.ConsumerId, consumerCommand, consumerQuery,
                marketQuery,
                tradingPairQueryRepository, userManager, redisDatabase, notifier, configurationDbContext, encryptionConfiguration.EncryptionKey, logger);

            result.Data.Add("redirectUrl", Url.Action("Edit", "Consumer"));
            result.Data.Add("message", "Updated Successfully");

            return result;
        }

        public async Task<bool> DeActive([FromRoute] int id, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deactivating consumer with ID: {ConsumerId}", id);

            try
            {
                var result =
                    await ConsumerHandlers.DeactivateAsync(id, consumerCommand, consumerQuery, redisDatabase, logger);
                logger.LogInformation("Consumer {ConsumerId} deactivated successfully: {Result}", id, result);
                return result;
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while deactivating consumer {ConsumerId}: {Message}", id,
                    ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while deactivating consumer {ConsumerId}", id);
                return false;
            }
        }

        [HttpPost]
        public async Task<ResponseWrapper<ConsumerResponse>> Activate([FromBody] ActivateConsumerModel model, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deactivating consumer with ID: {ConsumerId}", model.Id);
            var response = new ResponseWrapper<ConsumerResponse>();
            try
            {
                var result = await ConsumerHandlers.ActivateAsync(model.Id, model.IsActive, consumerCommand, consumerQuery, redisDatabase, logger);
                logger.LogInformation("Consumer {ConsumerId} deactivated successfully: {Result}", model.Id, result);

                if (result)
                {
                    response.IsSuccess = true;
                    response.Data.Add("message", "Updated Successfully");
                }
                else
                {
                    response.IsSuccess = false;
                    response.Data.Add("message", "update was unSuccessfully");
                }

                return response;
                   
            }
            catch (ApplicationBadRequestException ex)
            {
                logger.LogWarning(ex, "Bad request while deactivating consumer {ConsumerId}: {Message}", model.Id,
                    ex.Message);
                response.IsSuccess = false;
                response.Data.Add("message", ex.ToString());
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while deactivating consumer {ConsumerId}", model.Id);
                response.IsSuccess = false;
                response.Data.Add("message", "Something Wrong Please Check Logs.");
                return response;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateClientSecret([FromQuery] int consumerId, CancellationToken cancellationToken)
        {
            var result = await GenerateSecretAsync(consumerId, cancellationToken);
            return Json(new { clientId = result.clientId, clientSecret = result.clientSecret });
        }

        private async Task<(string clientId, string clientSecret)> GenerateSecretAsync(int consumerId, CancellationToken cancellationToken)
        {
            return await ConsumerHandlers.GenerateClientSecret(consumerId,
                consumerQuery,
                apiKeyClientService,
                encryptionConfiguration.EncryptionKey,
                cancellationToken);
        }

        private static List<SelectListItem> GenerateUserSelectList(List<User> users, List<string> selectedIds = null)
        {
            selectedIds ??= [];
            var result = new List<SelectListItem>();
            result.AddRange(users.Select(user => new SelectListItem
            {
                Value = user.Id.ToString(),
                Text = user.UserName,
                Selected = selectedIds.Contains(user.Id.ToString())
            }));

            return result;
        }
    }
}