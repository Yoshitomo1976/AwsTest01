# ユーザープール作成
aws cognito-idp create-user-pool --pool-name "Test user pool"

# ユーザープール確認
aws cognito-idp list-user-pools --max-results 10

# IDを変数にセットする
userpool_id=$(aws cognito-idp list-user-pools --max-results 10 --query 'UserPools[0].Id' --output text)
echo $userpool_id

# 固定変数作成
region="ap-northeast-1"
api_base_url="https://cphr17sdgi.execute-api.ap-northeast-1.amazonaws.com/Prod/"
callback_url="${api_base_url}swagger/oauth2-redirect.html"
logout_url="${api_base_url}swagger"
client_name="swagger-ui-client"

# Cognito Hosted UI 用のドメインを作成
domain_prefix="my-api-auth-$(date +%Y%m%d%H%M%S)"

aws cognito-idp create-user-pool-domain \
  --region "$region" \
  --user-pool-id "$userpool_id" \
  --domain "$domain_prefix" \
  --managed-login-version 1

cognito_domain="https://${domain_prefix}.auth.${region}.amazoncognito.com"

echo "$cognito_domain"
# ユーザープールクライアント作成
client_id=$(
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
    --prevent-user-existence-errors ENABLED \
    --query 'UserPoolClient.ClientId' \
    --output text
)

echo "$client_id"

# クライアントの内容を確認
aws cognito-idp describe-user-pool-client \
  --region "$region" \
  --user-pool-id "$userpool_id" \
  --client-id "$client_id" \
  --query 'UserPoolClient.{
    ClientId:ClientId,
    ClientName:ClientName,
    CallbackURLs:CallbackURLs,
    LogoutURLs:LogoutURLs,
    AllowedOAuthFlows:AllowedOAuthFlows,
    AllowedOAuthScopes:AllowedOAuthScopes,
    SupportedIdentityProviders:SupportedIdentityProviders,
    GenerateSecret:GenerateSecret
  }'

# ログインURLを生成
login_url="${cognito_domain}/oauth2/authorize?response_type=code&client_id=${client_id}&redirect_uri=${callback_url}&scope=openid+email+profile"
echo "$login_url"