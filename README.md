# Package installed

1. Dapper (ver 2.0.123)
2. System.Data.SqlClient (ver 4.8.3)
3. Microsoft.AspNetCore.Authentication.JwtBearer (ver 6.0.5)
4. Microsoft.Extensions.Configuration.Abstractions (ver 6.0.0)
5. Microsoft.EntityFrameworkCore (ver 6.0.6)
6. Newtonsoft.Json (ver 13.0.1)
7. Swashbuckle.AspNetCore (ver 6.3.1)
8. Microsoft.IdentityModel.Tokens (ver 6.19.0)
9. Microsoft.IdentityModel.JsonWebTokens (ver 6.19.0)
10. AutoMapper (ver 11.0.1)

# Run Redis auto startup

IOpen a Command Prompt window (cmd) with administrative rights. Move to the directory where Redis is located and run the following command:

```
redis-server --service-install redis.windows-service.conf
```

```
redis-server --service-start --service-name Redis
```

## Available Scripts

### `npm start`

### `npm test`

### `npm run build`

### `npm run eject`

**Note: this is a one-way operation. Once you `eject`, you can't go back!**

## Learn More

### Code Splitting

### Analyzing the Bundle Size

### Making a Progressive Web App

### Advanced Configuration

### Deployment

### `npm run build` fails to minify

This section has moved here: [https://facebook.github.io/create-react-app/docs/troubleshooting#npm-run-build-fails-to-minify](https://facebook.github.io/create-react-app/docs/troubleshooting#npm-run-build-fails-to-minify)
