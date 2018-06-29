using System.Web.Http;
using WebActivatorEx;
using Service.WebAPI;
using Swashbuckle.Application;
using System.Net.Http;
using System.Collections.Generic;
using Swashbuckle.Swagger;
using System;
using System.Web.Http.Description;
using System.Web.Http.Controllers;
using System.Reflection;
using System.Linq;
using Service.WebAPI.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Service.WebAPI
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        // By default, the service root url is inferred from the request used to access the docs.
                        // However, there may be situations (e.g. proxy and load-balanced environments) where this does not
                        // resolve correctly. You can workaround this by providing your own code to determine the root URL.
                        //
                        //c.RootUrl(req => GetRootUrlFromAppConfig());
                        c.RootUrl(ResolveBasePath);

                        // If schemes are not explicitly provided in a Swagger 2.0 document, then the scheme used to access
                        // the docs is taken as the default. If your API supports multiple schemes and you want to be explicit
                        // about them, you can use the "Schemes" option as shown below.
                        //
                        //c.Schemes(new[] { "http", "https" });

                        // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                        // hold additional metadata for an API. Version and title are required but you can also provide
                        // additional fields by chaining methods off SingleApiVersion.
                        //
                        c.SingleApiVersion("v1", "Web Api With Swagger Integration Example V1");

                        // If you want the output Swagger docs to be indented properly, enable the "PrettyPrint" option.
                        //
                        //c.PrettyPrint();

                        // If your API has multiple versions, use "MultipleApiVersions" instead of "SingleApiVersion".
                        // In this case, you must provide a lambda that tells Swashbuckle which actions should be
                        // included in the docs for a given API version. Like "SingleApiVersion", each call to "Version"
                        // returns an "Info" builder so you can provide additional metadata per API version.
                        //
                        //c.MultipleApiVersions(
                        //    (apiDesc, targetApiVersion) => ResolveVersionSupportByRouteConstraint(apiDesc, targetApiVersion),
                        //    (vc) =>
                        //    {
                        //        vc.Version("v2", "Swashbuckle Dummy API V2");
                        //        vc.Version("v1", "Swashbuckle Dummy API V1");
                        //    });

                        // You can use "BasicAuth", "ApiKey" or "OAuth2" options to describe security schemes for the API.
                        // See https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md for more details.
                        // NOTE: These only define the schemes and need to be coupled with a corresponding "security" property
                        // at the document or operation level to indicate which schemes are required for an operation. To do this,
                        // you'll need to implement a custom IDocumentFilter and/or IOperationFilter to set these properties
                        // according to your specific authorization implementation
                        //
                        //c.BasicAuth("basic")
                        //    .Description("Basic HTTP Authentication");
                        //
                        // NOTE: You must also configure 'EnableApiKeySupport' below in the SwaggerUI section
                        //c.ApiKey("apiKey")
                        //    .Description("API Key Authentication")
                        //    .Name("apiKey")
                        //    .In("header");
                        //
                        //c.OAuth2("oauth2")
                        //    .Description("OAuth2 Implicit Grant")
                        //    .Flow("implicit")
                        //    .AuthorizationUrl("http://petstore.swagger.wordnik.com/api/oauth/dialog")
                        //    //.TokenUrl("https://tempuri.org/token")
                        //    .Scopes(scopes =>
                        //    {
                        //        scopes.Add("read", "Read access to protected resources");
                        //        scopes.Add("write", "Write access to protected resources");
                        //    });

                        // Set this flag to omit descriptions for any actions decorated with the Obsolete attribute
                        //c.IgnoreObsoleteActions();

                        // Each operation be assigned one or more tags which are then used by consumers for various reasons.
                        // For example, the swagger-ui groups operations according to the first tag of each operation.
                        // By default, this will be controller name but you can use the "GroupActionsBy" option to
                        // override with any value.
                        //
                        //c.GroupActionsBy(apiDesc => apiDesc.HttpMethod.ToString());

                        // You can also specify a custom sort order for groups (as defined by "GroupActionsBy") to dictate
                        // the order in which operations are listed. For example, if the default grouping is in place
                        // (controller name) and you specify a descending alphabetic sort order, then actions from a
                        // ProductsController will be listed before those from a CustomersController. This is typically
                        // used to customize the order of groupings in the swagger-ui.
                        //
                        //c.OrderActionGroupsBy(new DescendingAlphabeticComparer());

                        // If you annotate Controllers and API Types with
                        // Xml comments (http://msdn.microsoft.com/en-us/library/b2s063f7(v=vs.110).aspx), you can incorporate
                        // those comments into the generated docs and UI. You can enable this by providing the path to one or
                        // more Xml comment files.
                        //
                        //c.IncludeXmlComments(GetXmlCommentsPath());
                        c.IncludeXmlComments(GetXmlCommentsPath());

                        // Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types
                        // exposed in your API. However, there may be occasions when more control of the output is needed.
                        // This is supported through the "MapType" and "SchemaFilter" options:
                        //
                        // Use the "MapType" option to override the Schema generation for a specific type.
                        // It should be noted that the resulting Schema will be placed "inline" for any applicable Operations.
                        // While Swagger 2.0 supports inline definitions for "all" Schema types, the swagger-ui tool does not.
                        // It expects "complex" Schemas to be defined separately and referenced. For this reason, you should only
                        // use the "MapType" option when the resulting Schema is a primitive or array type. If you need to alter a
                        // complex Schema, use a Schema filter.
                        //
                        //c.MapType<ProductType>(() => new Schema { type = "integer", format = "int32" });

                        // If you want to post-modify "complex" Schemas once they've been generated, across the board or for a
                        // specific type, you can wire up one or more Schema filters.
                        //
                        c.SchemaFilter<ApplySchemaVendorExtensions>();

                        // In a Swagger 2.0 document, complex types are typically declared globally and referenced by unique
                        // Schema Id. By default, Swashbuckle does NOT use the full type name in Schema Ids. In most cases, this
                        // works well because it prevents the "implementation detail" of type namespaces from leaking into your
                        // Swagger docs and UI. However, if you have multiple types in your API with the same class name, you'll
                        // need to opt out of this behavior to avoid Schema Id conflicts.
                        //
                        //c.UseFullTypeNameInSchemaIds();

                        // Alternatively, you can provide your own custom strategy for inferring SchemaId's for
                        // describing "complex" types in your API.
                        //
                        //c.SchemaId(t => t.FullName.Contains('`') ? t.FullName.Substring(0, t.FullName.IndexOf('`')) : t.FullName);

                        // Set this flag to omit schema property descriptions for any type properties decorated with the
                        // Obsolete attribute
                        //c.IgnoreObsoleteProperties();

                        // In accordance with the built in JsonSerializer, Swashbuckle will, by default, describe enums as integers.
                        // You can change the serializer behavior by configuring the StringToEnumConverter globally or for a given
                        // enum type. Swashbuckle will honor this change out-of-the-box. However, if you use a different
                        // approach to serialize enums as strings, you can also force Swashbuckle to describe them as strings.
                        //
                        //c.DescribeAllEnumsAsStrings();

                        // Similar to Schema filters, Swashbuckle also supports Operation and Document filters:
                        //
                        // Post-modify Operation descriptions once they've been generated by wiring up one or more
                        // Operation filters.
                        //
                        //c.OperationFilter<AddDefaultResponse>();
                        c.OperationFilter<AddDefaultValues>();
                        //
                        // If you've defined an OAuth2 flow as described above, you could use a custom filter
                        // to inspect some attribute on each action and infer which (if any) OAuth2 scopes are required
                        // to execute the operation
                        //
                        //c.OperationFilter<AssignOAuth2SecurityRequirements>();

                        // Post-modify the entire Swagger document by wiring up one or more Document filters.
                        // This gives full control to modify the final SwaggerDocument. You should have a good understanding of
                        // the Swagger 2.0 spec. - https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md
                        // before using this option.
                        //
                        //c.DocumentFilter<ApplyDocumentVendorExtensions>();

                        // In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL
                        // to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions
                        // with the same path (sans query string) and HTTP method. You can workaround this by providing a
                        // custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs
                        //
                        //c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                        // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
                        // alternative implementation for ISwaggerProvider with the CustomProvider option.
                        //
                        //c.CustomProvider((defaultProvider) => new CachingSwaggerProvider(defaultProvider));
                    })
                .EnableSwaggerUi(c =>
                    {
                        // Use the "DocumentTitle" option to change the Document title.
                        // Very helpful when you have multiple Swagger pages open, to tell them apart.
                        //
                        //c.DocumentTitle("My Swagger UI");

                        // Use the "InjectStylesheet" option to enrich the UI with one or more additional CSS stylesheets.
                        // The file must be included in your project as an "Embedded Resource", and then the resource's
                        // "Logical Name" is passed to the method as shown below.
                        //
                        //c.InjectStylesheet(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");

                        // Use the "InjectJavaScript" option to invoke one or more custom JavaScripts after the swagger-ui
                        // has loaded. The file must be included in your project as an "Embedded Resource", and then the resource's
                        // "Logical Name" is passed to the method as shown above.
                        //
                        //c.InjectJavaScript(thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");

                        // The swagger-ui renders boolean data types as a dropdown. By default, it provides "true" and "false"
                        // strings as the possible choices. You can use this option to change these to something else,
                        // for example 0 and 1.
                        //
                        //c.BooleanValues(new[] { "0", "1" });

                        // By default, swagger-ui will validate specs against swagger.io's online validator and display the result
                        // in a badge at the bottom of the page. Use these options to set a different validator URL or to disable the
                        // feature entirely.
                        //c.SetValidatorUrl("http://localhost/validator");
                        //c.DisableValidator();

                        // Use this option to control how the Operation listing is displayed.
                        // It can be set to "None" (default), "List" (shows operations for each resource),
                        // or "Full" (fully expanded: shows operations and their details).
                        //
                        //c.DocExpansion(DocExpansion.List);

                        // Specify which HTTP operations will have the 'Try it out!' option. An empty paramter list disables
                        // it for all operations.
                        //
                        //c.SupportedSubmitMethods("GET", "HEAD");

                        // Use the CustomAsset option to provide your own version of assets used in the swagger-ui.
                        // It's typically used to instruct Swashbuckle to return your version instead of the default
                        // when a request is made for "index.html". As with all custom content, the file must be included
                        // in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to
                        // the method as shown below.
                        //
                        c.CustomAsset("index", thisAssembly, "Service.WebAPI.SwashbuckleHTML.index.html");

                        // If your API has multiple versions and you've applied the MultipleApiVersions setting
                        // as described above, you can also enable a select box in the swagger-ui, that displays
                        // a discovery URL for each version. This provides a convenient way for users to browse documentation
                        // for different API versions.
                        //
                        c.EnableDiscoveryUrlSelector();

                        // If your API supports the OAuth2 Implicit flow, and you've described it correctly, according to
                        // the Swagger 2.0 specification, you can enable UI support as shown below.
                        //
                        //c.EnableOAuth2Support(
                        //    clientId: "test-client-id",
                        //    clientSecret: null,
                        //    realm: "test-realm",
                        //    appName: "Swagger UI"
                        //    //additionalQueryStringParams: new Dictionary<string, string>() { { "foo", "bar" } }
                        //);

                        // If your API supports ApiKey, you can override the default values.
                        // "apiKeyIn" can either be "query" or "header"
                        //
                        //c.EnableApiKeySupport("apiKey", "header");
                    });
        }

        /// <summary>
        /// Path to xml File
        /// </summary>
        /// <returns></returns>
        protected static string GetXmlCommentsPath()
        {
            return System.String.Format(@"{0}\bin\Service.WebAPI.XML", System.AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// Resolve server path correctly
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string ResolveBasePath(HttpRequestMessage message)
        {
            var virtualPathRoot = message.GetRequestContext().VirtualPathRoot;
            return string.Format("{0}://{1}:{2}{3}", message.RequestUri.Scheme, message.RequestUri.Host, message.RequestUri.Port, virtualPathRoot);
        }
    }

    /**** START :: SWAGGER DEFAULT VALUES ****/
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerDefaultValue : Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SwaggerDefaultValue(string value)
        {
            this.Value = value;
        }

        public SwaggerDefaultValue(string name, string value)
            : this(value)
        {
            this.Name = name;
        }
    }
    public class AddDefaultValues : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                return;
            IDictionary<string, object> parameterValuePairs =
            GetParameterValuePairs(apiDescription.ActionDescriptor);

            foreach (var param in operation.parameters)
            {
                if (param.schema != null && param.schema.@ref != null)
                {
                    string schemaName = param.schema.@ref.Split('/').LastOrDefault();
                    if (schemaRegistry.Definitions.ContainsKey(schemaName))
                        foreach (var props in schemaRegistry.Definitions[schemaName].properties)
                        {
                            if (parameterValuePairs.ContainsKey(props.Key))
                                props.Value.@default = parameterValuePairs[props.Key];
                        }
                }
                var parameterValuePair = parameterValuePairs.FirstOrDefault(p => p.Key.IndexOf(param.name, StringComparison.InvariantCultureIgnoreCase) >= 0);
                param.@default = parameterValuePair.Value;
            }
            if (operation.operationId == "Lore_GetReportPDF")
            {
                operation.produces = new[] { "application/octet-stream", "application/pdf", "application/json" };
                operation.responses["200"].schema = new Schema { type = "PDF File" };

            }
            else if (operation.operationId == "Lore_GetI7" || operation.operationId == "Lore_GetI71" || operation.operationId == "Lore_GetI72")
            {
                operation.produces = new[] { "application/octet-stream", "image/png", "application/json" };
                operation.responses["200"].schema = new Schema { type = "Image" };

            }
        }

        private IDictionary<string, object> GetParameterValuePairs(HttpActionDescriptor actionDescriptor)
        {
            IDictionary<string, object> parameterValuePairs = new Dictionary<string, object>();

            foreach (SwaggerDefaultValue defaultValue in actionDescriptor.GetCustomAttributes<SwaggerDefaultValue>())
            {
                parameterValuePairs.Add(defaultValue.Name, defaultValue.Value);
            }

            foreach (var parameter in actionDescriptor.GetParameters())
            {
                if (!parameter.ParameterType.IsPrimitive)
                {
                    foreach (PropertyInfo property in parameter.ParameterType.GetProperties())
                    {
                        var defaultValue = GetDefaultValue(property);

                        if (defaultValue != null)
                        {
                            parameterValuePairs.Add(property.Name, defaultValue);
                        }
                    }
                }
            }

            return parameterValuePairs;
        }

        private static object GetDefaultValue(PropertyInfo property)
        {
            var customAttribute = property.GetCustomAttributes<SwaggerDefaultValue>().FirstOrDefault();

            if (customAttribute != null)
            {
                return customAttribute.Value;
            }

            return null;
        }
    }
    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.IsArray && type.GetElementType() != typeof(byte)) return; // Special case

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);

            switch (type.Name)
            {
                case "Employee":
                    schema.example = (DefaultParameters.GetDefaultParam(type.Name)).ToCamelCaseObject(); ;
                    break;
            }
            schema.vendorExtensions.Add("x-type-dotnet", (nullableUnderlyingType ?? type).FullName);
            schema.vendorExtensions.Add("x-nullable", nullableUnderlyingType != null || !type.IsValueType);
        }
    }

    public static class ObjectAssistant
    {
        public static object ToCamelCaseObject(this object obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
            return JsonConvert.DeserializeObject(json);
        }
    }
    /**** END :: SWAGGER DEFAULT VALUES ****/
}
