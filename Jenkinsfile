pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    environment {
        REGISTRY = 'rutkre'
        IMAGE_NAME = 'fitbridge-be'
        DOCKER_BUILDKIT = '1' // use docker buildx
        // IMAGE_TAG=sh(script: 'git rev-parse HEAD | sha256sum | cut -d\' \' -f1', returnStdout: true).trim()
    }

    stages {
        stage('Build') {
            steps {
                echo "Building.."
                sh '''
                    dotnet build --property:WarningLevel=0 --configuration Release
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test --no-build --configuration Release
                '''
            }
        }
        stage('Build & Push') {
            steps {
                script {
                    withCredentials([usernamePassword(
                        credentialsId: 'docker-credentials',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )]) {
                        try {
                            echo 'Authenticating...'
                            // Authenticate through docker hub w/ buildkit
                            sh '''
                                mkdir -p ~/.docker
                                AUTH=$(echo -n "$DOCKER_USER:$DOCKER_PASS" | base64)
                                echo "{\\"auths\\":{\\"https://index.docker.io/v1/\\":{\\"auth\\":\\"$AUTH\\"}}}" > ~/.docker/config.json
                            '''
                            // Build and push
                            echo 'Building and pushing...'
                            sh '''
                                IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1)
                                docker buildx build \
                                    --progress=plain \
                                    --push \
                                    -t rutkre/fitbridge-be:${IMAGE_TAG} \
                                    -t rutkre/fitbridge-be:latest \
                                    --build-arg BUILDKIT_INLINE_CACHE=1 \
                                    --cache-from rutkre/fitbridge-be:latest \
                                    .
                            '''
                        } catch (Exception e) {
                            error e.getMessage()
                        } finally {
                            // Cleanup
                            echo 'Cleaning up...'
                            sh 'docker logout https://index.docker.io/v1/ && rm -f ~/.docker/config.json'
                        }
                    }
                }
            }
        }
        stage('Deploy') {
            steps {
                script {
                    env.MY_VAR = sh(script: 'docker images rutkre/fitbridge-be --format "{{.Tag}}" | head -n 1', returnStdout: true).trim()
                    try {
                        echo "Deploying..."
                        sshagent(['velour-ssh']) {
                            sh '''
                                ssh velour@ssh.velour-pie.io.vn "
                                    cd ~/deploy/stacks && \
                                    IMAGE_TAG=$(git rev-parse HEAD | sha256sum | cut -d' ' -f1) 
                                    docker compose --env-file /home/velour/deploy/.voyager.env down api-fitbridge && \
                                    IMAGE_TAG=${IMAGE_TAG} docker compose --env-file /home/velour/deploy/.voyager.env up api-fitbridge --remove-orphans -d 
                                "
                            '''
                        }
                    } catch (Exception e) {
                        echo 'Error during deployment: ' + e.getMessage()
                        echo 'Reverting back to old version...'
                        sshagent(['velour-ssh']) {
                            sh '''
                                ssh velour@ssh.velour-pie.io.vn "
                                    cd ~/deploy/stacks && \
                                    docker compose --env-file /home/velour/deploy/.voyager.env down api-fitbridge && \
                                    IMAGE_TAG=${MY_VAR} docker compose --env-file /home/velour/deploy/.voyager.env up api-fitbridge --remove-orphans -d 
                                "
                            '''
                        }
                    } 
                }
            }
        }
    }
}
