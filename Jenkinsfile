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
                    def imagePath = "rutkre/fitbridge-be"
                    def credsId = 'docker-credentials'

                    withCredentials([usernamePassword(credentialsId: credsId, usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        sh '''
                            echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin
                            
                            # Build with buildx using inline cache
                            docker buildx build \
                                -t ${REGISTRY}/${IMAGE_NAME}:${BUILD_NUMBER} \
                                -t ${REGISTRY}/${IMAGE_NAME}:latest \
                                --build-arg BUILDKIT_INLINE_CACHE=1 \
                                --cache-from ${REGISTRY}/${IMAGE_NAME}:latest \
                                --push \
                                .
                            
                            docker logout
                        '''
                    }
                }
            }
        }
    }
}
