# Set the base image as the .NET 5.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./RSIDiffBot/RSIDiffBot.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Kara D. <lunarautomaton6@gmail.com>
LABEL repository="https://github.com/mirrorcult/RSIDiffBot"
LABEL homepage="https://github.com/mirrorcult/RSIDiffBot"

# Label as GitHub action
LABEL com.github.actions.name="RSI Diff Bot"
# Limit to 160 characters
LABEL com.github.actions.description="Shows modified states in any changed Robust Station Images."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="columns"
LABEL com.github.actions.color="green"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/RSIDiffBot.dll" ]
