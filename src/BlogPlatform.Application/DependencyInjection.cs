using BlogPlatform.Application.UseCases.Posts;
using BlogPlatform.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<BlogPost>();

        services.AddScoped<CreatePostUseCase>();
        services.AddScoped<GetAllPostsUseCase>();
        services.AddScoped<GetPostByIdUseCase>();

        return services;
    }
}
