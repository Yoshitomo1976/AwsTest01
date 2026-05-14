 
region=ap-northeast-1

# ユーザープール作成
aws cognito-idp create-user-pool \
    --region "$region" \
    --pool-name "my-api-user-pool" \
    --username-attributes email \
    --auto-verified-attributes email \
    --schema Name=email,AttributeDataType=String,Required=true,Mutable=true

userpool_id=$(aws cognito-idp list-user-pools --max-results 10 --query 'UserPools[0].Id' --output text)
echo $userpool_id

# Cognito Hosted UI 用のドメインを作成
domain_prefix="my-api-auth-$(date +%Y%m%d%H%M%S)"

aws cognito-idp create-user-pool-domain \
    --region "$region" \
    --user-pool-id "$userpool_id" \
    --domain "$domain_prefix" \
    --managed-login-version 1

cognito_domain="https://${domain_prefix}.auth.${region}.amazoncognito.com"
echo "$cognito_domain"

# クライアント作成
api_base_url="https://cphr17sdgi.execute-api.ap-northeast-1.amazonaws.com/Prod/"
callback_url="${api_base_url}oauth2-redirect.html"
logout_url="${api_base_url}"
client_name="swagger-ui-client"

aws cognito-idp create-user-pool-client \
    --region "$region" \
    --user-pool-id "$userpool_id" \
    --client-name "$client_name" \
    --no-generate-secret \
    --supported-identity-providers COGNITO \
    --callback-urls "$callback_url" \
    --logout-urls "$logout_url" \
    --allowed-o-auth-flows code \
    --allowed-o-auth-scopes openid email profile \
    --allowed-o-auth-flows-user-pool-client \
    --explicit-auth-flows ALLOW_USER_SRP_AUTH ALLOW_REFRESH_TOKEN_AUTH \
    --prevent-user-existence-errors ENABLED 

client_id=$(aws cognito-idp list-user-pool-clients --user-pool-id $userpool_id --query 'UserPoolClients[0].ClientId' --output text)
echo $client_id

