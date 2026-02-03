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
                    def imageTag = sh(script: "git rev-parse HEAD | sha256sum | cut -d' ' -f1", returnStdout: true)
                        .trim()
                    withCredentials([usernamePassword(
                        credentialsId: 'docker-credentials',
                        usernameVariable: 'DOCKER_USER',
                        passwordVariable: 'DOCKER_PASS'
                    )]) {
                        // Authenticate through docker hub w/ buildkit
                        sh '''
                            mkdir -p ~/.docker
                            AUTH=$(echo -n "$DOCKER_USER:$DOCKER_PASS" | base64)
                            echo "{\\"auths\\":{\\"https://index.docker.io/v1/\\":{\\"auth\\":\\"$AUTH\\"}}}" > ~/.docker/config.json
                        '''
                        
                        // Build and push
                        sh """
                            docker buildx build \
                                --push \
                                -t rutkre/fitbridge-be:${imageTag} \
                                -t rutkre/fitbridge-be:latest \
                                --build-arg BUILDKIT_INLINE_CACHE=1 \
                                --cache-from rutkre/fitbridge-be:latest \
                                .
                        """
                        
                        // Cleanup
                        sh 'rm -f ~/.docker/config.json'
                    }
                }
            }
        }
    }
}
